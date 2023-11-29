using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Editor
{
    /// <summary>
    /// A styler takes care of styling text in the editor (syntax highlighting)
    /// </summary>
    public interface IStyler
    {
        /// <summary>
        /// This is called by the text editor to apply styles to the text from startPos to endPos.
        /// All characters in this range must be styled. The startPos is always the first character of a line.
        /// </summary>
        void ApplyStyles(int startPos, int endPos);
    }
}
