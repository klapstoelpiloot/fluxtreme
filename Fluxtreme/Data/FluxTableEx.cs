using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Linq;

namespace CodeImp.Fluxtreme.Data
{
    public class FluxTableEx
    {
        /// <summary>
        /// The table data resulting from the flux query
        /// </summary>
        public FluxTable Data { get; private set; }

        /// <summary>
        /// The width of each column in pixels (in the same order as columns in FluxTable)
        /// </summary>
        public double[] ColumnWidths { get; private set; }

        // Constructor
        public FluxTableEx(FluxTable t)
        {
            Data = t;

            ColumnWidths = new double[t.Columns.Count];
            for (int c = 0; c < t.Columns.Count; c++)
            {
                int characterlength = t.Columns[c].Label.Length;
                foreach (FluxRecord r in t.Records)
                {
                    object v = r.GetValueByIndex(c);
                    if (v == null)
                    {
                        characterlength = Math.Max(characterlength, 5);
                    }
                    else
                    {
                        characterlength = Math.Max(characterlength, v.ToString().Length);
                    }
                }

                if (characterlength < 40)
                {
                    ColumnWidths[c] = 20 + characterlength * 7.5;
                }
                else
                {
                    ColumnWidths[c] = 300;
                }
            }
        }
    }
}
