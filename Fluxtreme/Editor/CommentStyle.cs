using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Editor
{
    public enum CommentStyle
    {
        /// <summary>
        /// Comments are just two forwards slashes: //
        /// </summary>
        Normal,

        /// <summary>
        /// Comments being with //* and end with *// at the end of the line.
        /// This works better for syntax highlighting in Grafana.
        /// </summary>
        Grafana
    }
}
