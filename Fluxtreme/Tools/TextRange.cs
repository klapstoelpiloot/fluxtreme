using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Tools
{
    /// <summary>
    /// Range in a text string. The start is inclusive, the end is exclusive.
    /// This meanings that when Start == End then Length = 0.
    /// </summary>
    public struct TextRange
    {
        public int Start;
        public int End;

        public int Length => End - Start;
    }
}
