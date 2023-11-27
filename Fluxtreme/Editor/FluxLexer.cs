using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Editor
{
    public class FluxLexer
    {
        private Scintilla editor;
        private FluxFunctionsDictionary functions;

        public FluxLexer(Scintilla editor)
        {
            this.editor = editor;
            editor.WordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            editor.WhitespaceChars = " \t\n\r";
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
            while(pos < endPos)
            {
                int c = editor.GetCharAt(pos);

                REPROCESS:
                switch(context)
                {
                    case FluxContext.None:
                        if(c == '/')
                        {
                            // This can be a divide operator or a comment...
                            // We must peek ahead to see the difference
                            if(editor.GetCharAt(pos + 1) == '/')
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
                        else if(c == '"')
                        {
                            stylebegin = pos;
                            context = FluxContext.String;
                        }
                        else if((c >= '0') && (c <= '9'))
                        {
                            stylebegin = pos;
                            context = FluxContext.Number;
                        }
                        else
                        {
                            editor.SetStyling(1, (int)FluxStyles.Default);
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
                        if(c == '\\')
                        {
                            // The next charatcer must be considered as part of the string as well
                            context = FluxContext.StringEscaped;
                        }
                        else if(c == '"')
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
    }
}
