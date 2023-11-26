using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Editor
{
    public enum FluxContext
    {
        Unknown,
        None,
        Comment,
        String,
        StringEscaped,
        Number
    }
}
