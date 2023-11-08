﻿using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluxtreme
{
    public class TableExtraData
    {
        /// <summary>
        /// The width of each column in pixels (in the same order as columns in FluxTable)
        /// </summary>
        public double[] ColumnWidths { get; private set; }

        // Constructor
        public TableExtraData()
        {
        }

        public static TableExtraData CalculateFor(FluxTable t)
        {
            TableExtraData ed = new TableExtraData();
            
            ed.ColumnWidths = new double[t.Columns.Count];
            for(int c = 0; c < t.Columns.Count; c++)
            {
                int characterlength = t.Columns[c].Label.Length;
                foreach(FluxRecord r in t.Records)
                {
                    characterlength = Math.Max(characterlength, r.GetValueByIndex(c).ToString().Length);
                }
                
                if(characterlength < 40)
                {
                    ed.ColumnWidths[c] = 20 + characterlength * 7.5;
                }
                else
                {
                    ed.ColumnWidths[c] = 300;
                }
            }

            return ed;
        }
    }
}