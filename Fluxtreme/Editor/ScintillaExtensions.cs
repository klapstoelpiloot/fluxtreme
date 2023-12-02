using CodeImp.Fluxtreme.Tools;
using ScintillaNET;
using System;
using System.Drawing;
using System.Linq;

namespace CodeImp.Fluxtreme.Editor
{
    public static class ScintillaExtensions
    {
        /// <summary>
        /// Alternate arguments for DirectMessage
        /// </summary>
        public static IntPtr DirectMessage(this Scintilla editor, int msg, int wParam, Color lParam)
        {
            return editor.DirectMessage(msg, new IntPtr(wParam), new IntPtr(lParam.ToArgb()));
        }

        /// <summary>
        /// Alternate arguments for DirectMessage
        /// </summary>
        public static IntPtr DirectMessage(this Scintilla editor, int msg, int wParam, int lParam)
        {
            return editor.DirectMessage(msg, new IntPtr(wParam), new IntPtr(lParam));
        }

        /// <summary>
        /// Get a range of text by TextRange
        /// </summary>
        public static string GetTextRange(this Scintilla editor, TextRange range)
        {
            return editor.GetTextRange(range.Start, range.Length);
        }

        /// <summary>
        /// Moves the position into the given direction while the charcter at that position matches the given set of characters
        /// The start position is not checked. The returned position is the last character that matches the given set of characters.
        /// </summary>
        public static int WalkWhileCharacterMatch(this Scintilla editor, int pos, SearchDirection direction, string characters)
        {
            int nextpos = pos + (int)direction;
            while ((nextpos >= 0) && (nextpos < editor.TextLength) && characters.Contains((char)editor.GetCharAt(nextpos)))
            {
                pos = nextpos;
                nextpos += (int)direction;
            }
            return pos;
        }
    }
}
