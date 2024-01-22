using CodeImp.Fluxtreme.Tools;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeImp.Fluxtreme.Editor
{
    /// <summary>
    /// Contains function and definitions for lexical analysis of the flux language.
    /// Supports FluxStyler and FluxAssistant.
    /// </summary>
    public static class FluxLexer
    {
        public const string IdentifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
        public const char IdentifierSeparator = '.';
        public const string WhitspaceChars = " \t\n\r";
        public const string NumberChars = "0123456789.ymowdhsuµn";
        public static readonly string[] Keywords = new []{ "and", "import", "option", "if", "or", "package", "builtin", "then", "not", "return", "testcase", "else", "exists" };
        public static readonly string[] Operators = new []{ "+", "==", "!=", "=>", "-", "<", "!~", "^", "*", ">", "=~", "/", "<=", "=", "%", ">=", "<-", "|>" };
        public static readonly string OperatorChars = new string(string.Concat(Operators).Distinct().ToArray());

        public static IDictionary<string, IReadOnlyList<string>> Functions { get; private set; } = FluxFunctionsDictionary.FromResource();

        // Returns the text range that covers the whole operator at the given position.
        // This method assumes that the character at pos is an operator character and should be included.
        public static TextRange OperatorFromPosition(Scintilla editor, int pos)
        {
            TextRange r = new TextRange();
            r.Start = editor.WalkWhileCharacterMatch(pos, SearchDirection.Backward, OperatorChars);
            r.End = editor.WalkWhileCharacterMatch(pos, SearchDirection.Forward, OperatorChars) + 1;
            return r;
        }

        // Returns the text range that covers the whole identifier name at the given position.
        // This includes parts separated by a dot (like the library a function is in).
        public static TextRange IdentifierFromPosition(Scintilla editor, int pos)
        {
            TextRange r = new TextRange();
            r.Start = editor.WalkWhileCharacterMatch(pos, SearchDirection.Backward, IdentifierChars + IdentifierSeparator);
            r.End = editor.WalkWhileCharacterMatch(pos, SearchDirection.Forward, IdentifierChars + IdentifierSeparator) + 1;
            return r;
        }

        /// <summary>
        /// This finds out in which function call the given position is.
        /// Returns null when it was not able to find the function name.
        /// </summary>
        public static string FunctionFromPosition(Scintilla editor, int pos)
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
                            TextRange idr = IdentifierFromPosition(editor, pos - 1);
                            string id = editor.GetTextRange(idr);
                            if (Functions.ContainsKey(id))
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
            while (pos > 0);

            // Can't find the function
            return null;
        }

        /// <summary>
        /// Returns a list of all identifiers styled as 'variable'.
        /// This requires all text to be styled.
        /// </summary>
        public static List<string> FindAllVariables(Scintilla editor, bool ignoreAtCursor)
        {
            List<string> vars = new List<string>();

            int pos = 0;
            while (pos < editor.TextLength)
            {
                if (editor.GetStyleAt(pos) == (int)FluxStyles.Variable)
                {
                    TextRange r = IdentifierFromPosition(editor, pos);
                    if (!ignoreAtCursor || (editor.CurrentPosition < r.Start) || (editor.CurrentPosition > r.End))
                    {
                        string v = editor.GetTextRange(r);
                        if (!vars.Contains(v))
                        {
                            vars.Add(v);
                        }
                    }
                    pos += r.Length;
                }

                pos++;
            }

            return vars;
        }
    }
}
