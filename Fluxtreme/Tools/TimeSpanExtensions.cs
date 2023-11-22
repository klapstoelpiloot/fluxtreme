using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Tools
{
    public static class TimeSpanExtensions
    {
        public static string ToShortString(this TimeSpan t)
        {
            string desc = "";
            if (t.Days > 0)
            {
                desc += t.Days + "d ";
            }
            if (t.Hours > 0)
            {
                desc += t.Hours + "h ";
            }
            if ((t.Days == 0) && (t.Minutes > 0))
            {
                desc += t.Minutes + "m ";
            }
            if ((t.Hours == 0) && (t.Days == 0) && (t.Seconds > 0))
            {
                desc += t.Seconds + "s ";
            }
            if ((t.Minutes == 0) && (t.Hours == 0) && (t.Days == 0) && (t.Milliseconds > 0))
            {
                desc += t.Milliseconds + "ms";
            }
            return desc.Trim();
        }
    }
}
