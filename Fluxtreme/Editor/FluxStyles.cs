using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Editor
{
    public enum FluxStyles : int
    {
        // See https://www.scintilla.org/ScintillaDoc.html#Styling
        Default = 0,
        Comment = 1,                // Starts with // and continue to the end of the line
        String = 2,
        Number = 3,                 // Also includes durations
        Function = 4,
        Parameter = 5,
        Keyword = 6,
        Variable = 7
    }
}
