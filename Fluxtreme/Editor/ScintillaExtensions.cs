using CodeImp.Fluxtreme.Tools;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Editor
{
    public static class ScintillaExtensions
    {
        public static string GetTextRange(this Scintilla editor, TextRange range)
        {
            return editor.GetTextRange(range.Start, range.Length);
        }
    }
}
