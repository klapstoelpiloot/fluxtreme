using CodeImp.Fluxtreme.Tools;
using ScintillaNET;
using System;
using System.Linq;

namespace CodeImp.Fluxtreme.Editor
{
    public class FluxLexer
    {
        private const string IdentifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        private const string WhitspaceChars = " \t\n\r";

        private Scintilla editor;
        private FluxFunctionsDictionary functions;

        public FluxLexer(Scintilla editor)
        {
            this.editor = editor;
            editor.WordChars = IdentifierChars;
            editor.WhitespaceChars = WhitspaceChars;
            functions = FluxFunctionsDictionary.FromResource();
        }

        /// <summary>
        /// This applies styling for the given range.
        /// The startPos is always at the start of a line.
        /// </summary>
        public void ApplyStyles(int startPos, int endPos)
        {
            // Parse text in range and apply styling
            FluxContext context = FluxContext.None;
            editor.StartStyling(startPos);
            int stylebegin = 0;
            int pos = startPos;
            while (pos < endPos)
            {
                int c = editor.GetCharAt(pos);

                REPROCESS:
                switch (context)
                {
                    case FluxContext.None:
                        if (c == '/')
                        {
                            // This can be a divide operator or a comment...
                            // We must peek ahead to see the difference
                            if (editor.GetCharAt(pos + 1) == '/')
                            {
                                context = FluxContext.Comment;
                                goto REPROCESS;
                            }
                            else
                            {
                                // TODO
                                editor.SetStyling(1, (int)FluxStyles.Default);
                            }
                        }
                        else if (c == '"')
                        {
                            stylebegin = pos;
                            context = FluxContext.String;
                        }
                        else if ((c >= '0') && (c <= '9'))
                        {
                            stylebegin = pos;
                            context = FluxContext.Number;
                        }
                        else if (IdentifierChars.Contains((char)c))
                        {
                            stylebegin = pos;
                            context = FluxContext.Identifier;
                        }
                        else
                        {
                            editor.SetStyling(1, (int)FluxStyles.Default);
                        }
                        break;

                    case FluxContext.Identifier:
                        if (!IdentifierChars.Contains((char)c))
                        {
                            TextRange idr = IdentifierFromPosition((stylebegin + pos) / 2);
                            string id = editor.GetTextRange(idr);

                            // Check if the identifier is a function call
                            if (functions.ContainsKey(id) && (editor.GetCharAt(NextNonWhitespace(idr.End)) == '('))
                            {
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Function);
                            }
                            else
                            {
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Default);
                            }
                            context = FluxContext.None;
                            goto REPROCESS;
                        }
                        break;

                    case FluxContext.Number:
                        // This includes floating points and durations
                        // TODO: Add date/time support to this?
                        if (((c < '0') || (c > '9')) && (c != '.') && (c != 'y') && (c != 'm') && (c != 'o') &&
                            (c != 'w') && (c != 'd') && (c != 'h') && (c != 's') && (c != 'u') && (c != 'µ') && (c != 'n'))
                        {
                            // Number/duration ends here
                            editor.SetStyling(pos - stylebegin, (int)FluxStyles.Number);
                            context = FluxContext.None;
                            goto REPROCESS;
                        }
                        break;

                    case FluxContext.String:
                        if (c == '\\')
                        {
                            // The next charatcer must be considered as part of the string as well
                            context = FluxContext.StringEscaped;
                        }
                        else if (c == '"')
                        {
                            editor.SetStyling(pos - stylebegin + 1, (int)FluxStyles.String);
                            context = FluxContext.None;
                        }
                        break;

                    case FluxContext.StringEscaped:
                        // This is just used to ignore one character,
                        // go back to the string context for the next character.
                        context = FluxContext.String;
                        break;

                    case FluxContext.Comment:
                        // Style as comment until the end of the line
                        int line = editor.LineFromPosition(pos);
                        int numchars = editor.Lines[line].EndPosition - pos;
                        editor.SetStyling(numchars, (int)FluxStyles.Comment);
                        context = FluxContext.None;
                        pos += numchars - 1;
                        break;

                }

                pos++;
            }
        }

        // Returns the next position (forward) that is not a whitespace character, starting from the given position.
        // The start position does not have to be a whitespace character. This method starts checking at the NEXT character.
        // If the next character after the given position is not a whitespace character, this returns pos + 1.
        private int NextNonWhitespace(int pos)
        {
            do
            {
                pos++;
            }
            while(WhitspaceChars.Contains((char)editor.GetCharAt(pos)));
            return pos;
        }

        // Returns the previous position (backwards) that is not a whitespace character, starting from the given position.
        // The start position does not have to be a whitespace character. This method starts checking at the PREVIOUS character.
        // If the previous character before the given position is not a whitespace character, this returns pos - 1.
        private int PrevNonWhitespace(int pos)
        {
            do
            {
                pos--;
            }
            while(WhitspaceChars.Contains((char)editor.GetCharAt(pos)));
            return pos;
        }

        // Returns the text range that covers the whole identifier name at the given position.
        // This includes parts separated by a dot (like the library a function is in).
        private TextRange IdentifierFromPosition(int pos)
        {
            TextRange r = new TextRange();
            r.Start = editor.WordStartPosition(pos, true);
            r.End = editor.WordEndPosition(pos, true) - 1;

            // Dot and identifier before the start? Then include it.
            if((editor.GetCharAt(r.Start - 1) == '.') && IdentifierChars.Contains((char)editor.GetCharAt(r.Start - 2)))
            {
                r.Start = editor.WordStartPosition(r.Start - 2, true);
            }

            // Dot and identifier after the end? Then include it.
            if((editor.GetCharAt(r.End + 1) == '.') && IdentifierChars.Contains((char)editor.GetCharAt(r.End + 2)))
            {
                r.End = editor.WordEndPosition(r.End + 2, true) - 1;
            }

            return r;
        }

        // This finds out in which function call the given position is.
        // Returns null when it was not able to find the function name.
        public string GetFunctionFromPosition(int pos)
        {
            // Find the first function identifier before the pos.
            // There should be an opening brace ( before the pos where we can find the function name.
            // If we find a closing brace ) then we have to skip over that scope/function.
            int nestcount = 0;
            do
            {
                pos--;
                int c = editor.GetCharAt(pos);
                FluxStyles style = (FluxStyles)editor.GetStyleAt(pos);

                // This relys on syntax highlighting to be correct and up-to-date.
                // Maybe not the best idea, but easier than determining this from the current position.
                if ((style != FluxStyles.Comment) && (style != FluxStyles.String))
                {
                    if (c == ')')
                    {
                        nestcount++;
                    }
                    else if (c == '(')
                    {
                        if (nestcount == 0)
                        {
                            // Now we expect the next to be the function identifier
                            TextRange idr = IdentifierFromPosition(pos - 1);
                            string id = editor.GetTextRange(idr);
                            if (functions.ContainsKey(id))
                            {
                                return id;
                            }
                            else
                            {
                                // Not a known function
                                return null;
                            }
                        }
                        else
                        {
                            nestcount--;
                        }
                    }
                }
            }
            while(pos > 0);

            // Can't find the function
            return null;
        }
    }
}
