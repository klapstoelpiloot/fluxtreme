using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Editor
{
    public enum FluxContext
    {
        /// <summary>
        /// Undetermined
        /// </summary>
        Unknown,

        None,

        /// <summary>
        /// Comment (starts with // and continues to the end of the line)
        /// </summary>
        Comment,

        /// <summary>
        /// String enclosed by double quotes
        /// </summary>
        String,

        /// <summary>
        /// Only used to prevent ending the String context on the next character
        /// </summary>
        StringEscaped,

        /// <summary>
        /// Integer, floating point or duration
        /// </summary>
        Number,

        /// <summary>
        /// Keyword, variable, function or parameter
        /// </summary>
        Identifier,

        /// <summary>
        /// Usually 1 or 2 special characters
        /// </summary>
        Operator,

        /// <summary>
        /// Regular expression. Starts and ends with /
        /// </summary>
        RegEx,

        /// <summary>
        /// Only used to prevent ending the RegEx context on the next character
        /// </summary>
        RegExEscaped
    }
}
