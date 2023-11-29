using CodeImp.Fluxtreme.Tools;
using CsvHelper.Configuration.Attributes;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Web.UI;
using static System.Net.Mime.MediaTypeNames;

namespace CodeImp.Fluxtreme.Editor
{
    public class FluxLexer
    {
        private const string IdentifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        private const string WhitspaceChars = " \t\n\r";
        private static readonly string[] Keywords = new []{ "and", "import", "option", "if", "or", "package", "builtin", "then", "not", "return", "testcase", "else", "exists" };
        private static readonly string[] Operators = new []{ "+", "==", "!=", "=>", "-", "<", "!~", "^", "*", ">", "=~", "/", "<=", "=", "%", ">=", "<-", "|>" };
        private static readonly string OperatorChars = new string(string.Concat(Operators).Distinct().ToArray());
        private enum SearchDirection { Forward = 1, Backward = -1 };

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
                            // This can be a divide, regex or a comment.
                            // Double forward slash is easy, that is the start of a comment.
                            if (editor.GetCharAt(pos + 1) == '/')
                            {
                                context = FluxContext.Comment;
                                goto REPROCESS;
                            }
                            else
                            {
                                // Requirements for regex:
                                // On the left is an operator or ':'
                                // On the right is a valid regex character: a-z, A-Z, 0-9, [, \, ^, (, . or whitespace
                                int prev = WalkWhileCharacterMatch(pos, SearchDirection.Backward, WhitspaceChars) - 1;
                                int prevc = editor.GetCharAt(prev);
                                if ((prevc == ':') || OperatorChars.Contains((char)prevc))
                                {
                                    // Begin regular expression
                                    stylebegin = pos;
                                    context = FluxContext.RegEx;
                                }
                                else
                                {
                                    // Divide operator
                                    stylebegin = pos;
                                    context = FluxContext.Operator;
                                }
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
                        else if (OperatorChars.Contains((char)c))
                        {
                            stylebegin = pos;
                            context = FluxContext.Operator;
                        }
                        else
                        {
                            editor.SetStyling(1, (int)FluxStyles.Default);
                        }
                        break;

                    case FluxContext.RegEx:
                        if (c == '\\')
                        {
                            // The next charatcer must be considered as part of the regex as well
                            context = FluxContext.RegExEscaped;
                        }
                        else if (c == '/')
                        {
                            editor.SetStyling(pos - stylebegin + 1, (int)FluxStyles.RegEx);
                            context = FluxContext.None;
                        }
                        break;

                    case FluxContext.RegExEscaped:
                        // This is just used to ignore one character,
                        // go back to the RegEx context for the next character.
                        context = FluxContext.RegEx;
                        break;

                    case FluxContext.Operator:
                        if (!OperatorChars.Contains((char)c))
                        {
                            TextRange opr = OperatorFromPosition((stylebegin + pos) / 2);
                            string op = editor.GetTextRange(opr);
                            if (Operators.Contains(op))
                            {
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Operator);
                            }
                            else
                            {
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Default);
                            }

                            context = FluxContext.None;
                            goto REPROCESS;
                        }
                        break;

                    case FluxContext.Identifier:
                        if (!IdentifierChars.Contains((char)c))
                        {
                            TextRange idr = IdentifierFromPosition((stylebegin + pos) / 2);
                            string id = editor.GetTextRange(idr);
                            int next = WalkWhileCharacterMatch(idr.End - 1, SearchDirection.Forward, WhitspaceChars) + 1;
                            int nextc = editor.GetCharAt(next);

                            // Check if the identifier is a function call (function name)
                            if ((nextc == '(') && functions.ContainsKey(id))
                            {
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Function);
                            }
                            // Check if it is a parameter
                            else if(nextc == ':')
                            {
                                // To check if this is a valid parameter, we need to know the function call
                                string func = GetFunctionFromPosition(idr.Start);
                                if(func != null)
                                {
                                    IReadOnlyList<string> args = functions[func];
                                    if(args.Contains(id))
                                    {
                                        editor.SetStyling(pos - stylebegin, (int)FluxStyles.Parameter);
                                    }
                                    else
                                    {
                                        // Don't know
                                        editor.SetStyling(pos - stylebegin, (int)FluxStyles.Default);
                                    }
                                }
                                else
                                {
                                    // Don't know, possibly a key in a key-value pair for a record/dictionary
                                    editor.SetStyling(pos - stylebegin, (int)FluxStyles.Default);
                                }
                            }
                            // Check if this is a language keyword
                            else if(Keywords.Contains(id))
                            {
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Keyword);
                            }
                            else
                            {
                                // Most likely a variable then
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Variable);
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

        // Moves the position into the given direction while the charcter at that position matches the given set of characters
        // The start position is not checked. The returned position is the last character that matches the given set of characters.
        private int WalkWhileCharacterMatch(int pos, SearchDirection direction, string characters)
        {
            int nextpos = pos + (int)direction;
            while((nextpos >= 0) && (nextpos < editor.TextLength) && characters.Contains((char)editor.GetCharAt(nextpos)))
            {
                pos = nextpos;
                nextpos += (int)direction;
            }
            return pos;
        }

        // Returns the text range that covers the whole operator at the given position.
        // This method assumes that the character at pos is an operator character and should be included.
        private TextRange OperatorFromPosition(int pos)
        {
            TextRange r = new TextRange();
            r.Start = WalkWhileCharacterMatch(pos, SearchDirection.Backward, OperatorChars);
            r.End = WalkWhileCharacterMatch(pos, SearchDirection.Forward, OperatorChars) + 1;
            return r;
        }

        // Returns the text range that covers the whole identifier name at the given position.
        // This includes parts separated by a dot (like the library a function is in).
        private TextRange IdentifierFromPosition(int pos)
        {
            TextRange r = new TextRange();
            r.Start = WalkWhileCharacterMatch(pos, SearchDirection.Backward, IdentifierChars + ".");
            r.End = WalkWhileCharacterMatch(pos, SearchDirection.Forward, IdentifierChars + ".") + 1;
            return r;
        }
        

        // This finds out in which function call the given position is.
        // Returns null when it was not able to find the function name.
        public string GetFunctionFromPosition(int pos)
        {
            // Find the first function identifier before the pos.
            // There should be an opening brace ( before the pos where we can find the function name.
            // If we find a closing brace ) then we have to skip over that scope/function.
            int functionnestcount = 0;
            int recordnestcount = 0;
            do
            {
                pos--;
                int c = editor.GetCharAt(pos);
                FluxStyles style = (FluxStyles)editor.GetStyleAt(pos);

                // This relies on syntax highlighting to be correct and up-to-date.
                // Maybe not the best idea, but easier than determining this by parsing backwards.
                if ((style != FluxStyles.Comment) && (style != FluxStyles.String))
                {
                    if (c == ')')
                    {
                        functionnestcount++;
                    }
                    else if (c == '(')
                    {
                        if (functionnestcount == 0)
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
                            functionnestcount--;
                        }
                    }
                    else if (c == '}')
                    {
                        recordnestcount++;
                    }
                    else if (c == '{')
                    {
                        if (recordnestcount == 0)
                        {
                            // We're in a dictionary/record
                            // This is not part of the function call
                            return null;
                        }
                        else
                        {
                            recordnestcount--;
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
