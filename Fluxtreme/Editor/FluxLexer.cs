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

        public FluxLexer(Scintilla editor)
        {
            this.editor = editor;
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
                        else
                        {
                            editor.SetStyling(1, (int)FluxStyles.Default);
                        }
                        pos++;
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
                        pos++;
                        break;

                    case FluxContext.StringEscaped:
                        // This is just used to ignore one character,
                        // go back to the string context for the next character.
                        context = FluxContext.String;
                        pos++;
                        break;

                    case FluxContext.Comment:
                        // Style as comment until the end of the line
                        int line = editor.LineFromPosition(pos);
                        int numchars = editor.Lines[line].EndPosition - pos;
                        editor.SetStyling(numchars, (int)FluxStyles.Comment);
                        context = FluxContext.None;
                        pos += numchars;
                        break;

                }
            }
        }
    }
}
