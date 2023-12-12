using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace CodeImp.Fluxtreme.Viewers
{
    public class FluxTableView : INotifyPropertyChanged
    {
        private const int CollapsedRowCount = 3;
        private const int MaxColumnChars = 40;
        private const int MinColumnChars = 5;
        private const double MaxColumnWidth = 300.0;
        private const double ColumnCharWidthFactor = 7.5;
        private const double ColumnCharWidthPadding = 20.0;
        private DataTable collapsedtable;
        private DataTable expandedtable;

        /// <summary>
        /// True when the DataTable is expanded (DataTable contains all or truncated rows)
        /// False when the DataTable is collapsed (limited to CollapsedRowCount)
        /// </summary>
        public bool Expanded { get; private set; }

        /// <summary>
        /// True when this list can be expanded (it contains more than CollapsedRowCount)
        /// </summary>
        public bool Expandable { get; private set; }

        /// <summary>
        /// Title to display for this table.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The contents of this table.
        /// </summary>
        public DataTable DataTable { get; private set; }

        /// <summary>
        /// Column widths in pixels for this table.
        /// </summary>
        public double[] ColumnWidths { get; private set; }

        /// <summary>
        /// Total number of rows (records) in this table.
        /// </summary>
        public int TotalRowCount { get; private set; }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor
        public FluxTableView(FluxTable table)
        {
            TotalRowCount = table.Records.Count;

            // Show collapsed or expanded?
            Expandable = TotalRowCount > CollapsedRowCount;
            Expanded = !Expandable;

            // Make up title
            StringBuilder titleBuilder = new StringBuilder();
            if (TotalRowCount > 0)
            {
                foreach (FluxColumn c in table.GetGroupKey())
                {
                    string colvalue = table.Records[0].Values[c.Label].ToString();
                    titleBuilder.Append(c.Label + ": " + colvalue + "   ");
                }
            }
            else
            {
                foreach (FluxColumn c in table.GetGroupKey())
                {
                    titleBuilder.Append(c.Label + "   ");
                }
            }
            Title = titleBuilder.ToString();

            // Calculate column widths
            ColumnWidths = new double[table.Columns.Count];
            for (int c = 0; c < table.Columns.Count; c++)
            {
                int characterlength = MinColumnChars;
                characterlength = Math.Max(characterlength, table.Columns[c].Label.Length);
                foreach (FluxRecord r in table.Records)
                {
                    object v = r.GetValueByIndex(c);
                    if (v != null)
                    {
                        characterlength = Math.Max(characterlength, v.ToString().Length);
                    }
                }

                if (characterlength < MaxColumnChars)
                {
                    ColumnWidths[c] = ColumnCharWidthPadding + characterlength * ColumnCharWidthFactor;
                }
                else
                {
                    ColumnWidths[c] = MaxColumnWidth;
                }
            }

            // Make a DataTable
            expandedtable = new DataTable(Title);
            collapsedtable = new DataTable(Title);
            foreach (FluxColumn c in table.Columns)
            {
                expandedtable.Columns.Add(c.Label);
                collapsedtable.Columns.Add(c.Label);
            }
            foreach (FluxRecord r in table.Records.Take(Math.Min(CollapsedRowCount, TotalRowCount)))
            {
                collapsedtable.Rows.Add(r.Values.Values.ToArray());
            }
            foreach (FluxRecord r in table.Records)
            {
                expandedtable.Rows.Add(r.Values.Values.ToArray());
            }
            DataTable = Expanded ? expandedtable : collapsedtable;
        }

        /// <summary>
        /// Toggles this table between expanded and collapsed view
        /// </summary>
        public void ToggleExpand()
        {
            if (Expandable)
            {
                Expanded = !Expanded;
                DataTable = Expanded ? expandedtable : collapsedtable;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Expanded)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataTable)));
            }
        }
    }
}
