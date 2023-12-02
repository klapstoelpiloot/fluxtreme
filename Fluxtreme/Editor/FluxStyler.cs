using CodeImp.Fluxtreme.Tools;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeImp.Fluxtreme.Editor
{
    /// <summary>
    /// Takes care of styling text in the editor (syntax highlighting)
    /// </summary>
    public class FluxStyler : IStyler
    {
        private Scintilla editor;

        // Constructor
        public FluxStyler(Scintilla editor)
        {
            this.editor = editor;
            editor.WordChars = FluxLexer.IdentifierChars;
            editor.WhitespaceChars = FluxLexer.WhitspaceChars;
        }

        /// <inheritdoc/>
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
                                int prev = editor.WalkWhileCharacterMatch(pos, SearchDirection.Backward, FluxLexer.WhitspaceChars) - 1;
                                int prevc = editor.GetCharAt(prev);
                                if ((prevc == ':') || FluxLexer.OperatorChars.Contains((char)prevc))
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
                        else if (FluxLexer.IdentifierChars.Contains((char)c))
                        {
                            stylebegin = pos;
                            context = FluxContext.Identifier;
                        }
                        else if (FluxLexer.OperatorChars.Contains((char)c))
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
                        else if ((c == '/') || (c == '\n'))
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
                        if (!FluxLexer.OperatorChars.Contains((char)c))
                        {
                            TextRange opr = FluxLexer.OperatorFromPosition(editor, (stylebegin + pos) / 2);
                            string op = editor.GetTextRange(opr);
                            if (FluxLexer.Operators.Contains(op))
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
                        if (!FluxLexer.IdentifierChars.Contains((char)c))
                        {
                            TextRange idr = FluxLexer.IdentifierFromPosition(editor, (stylebegin + pos) / 2);
                            string id = editor.GetTextRange(idr);
                            int next = editor.WalkWhileCharacterMatch(idr.End - 1, SearchDirection.Forward, FluxLexer.WhitspaceChars) + 1;
                            int nextc = editor.GetCharAt(next);

                            // Check if the identifier is a function call (function name)
                            if ((nextc == '(') && FluxLexer.Functions.ContainsKey(id))
                            {
                                editor.SetStyling(pos - stylebegin, (int)FluxStyles.Function);
                            }
                            // Check if it is a parameter
                            else if (nextc == ':')
                            {
                                // To check if this is a valid parameter, we need to know the function call
                                string func = FluxLexer.FunctionFromPosition(editor, idr.Start);
                                if (func != null)
                                {
                                    IReadOnlyList<string> args = FluxLexer.Functions[func];
                                    if (args.Contains(id))
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
                            else if (FluxLexer.Keywords.Contains(id))
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
                        if (!FluxLexer.NumberChars.Contains((char)c))
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
                        else if ((c == '"') || (c == '\n'))
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
