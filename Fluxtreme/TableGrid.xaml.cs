using InfluxDB.Client.Core.Flux.Domain;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Fluxtreme
{
    /// <summary>
    /// Interaction logic for TableGrid.xaml
    /// </summary>
    public partial class TableGrid : UserControl
    {
        private FluxTable fluxtable;
        private DataTable datatable;

        public TableGrid()
        {
            InitializeComponent();
        }

        public TableGrid(FluxTable fluxtable)
        {
            this.fluxtable = fluxtable;

            // Build a DataTable from FluxTable
            datatable = new DataTable();
            foreach (FluxColumn c in fluxtable.Columns)
            {
                datatable.Columns.Add(c.Label);
            }
            foreach (FluxRecord r in fluxtable.Records)
            {
                datatable.Rows.Add(r.Values.Values.ToArray());
            }

            InitializeComponent();

            // Apply contents
            tablecontents.ItemsSource = datatable.DefaultView;

            // Make up table name
            foreach (FluxColumn c in fluxtable.GetGroupKey())
            {
                string colvalue = fluxtable.Records[0].Values[c.Label].ToString();

                tablename.Inlines.Add(new Run(c.Label + ":") { FontWeight = FontWeights.Bold });
                tablename.Inlines.Add(new Run(" " + colvalue) { FontWeight = FontWeights.Normal });
                tablename.Inlines.Add(new Run("   "));
            }
        }

        // Called when columns are generated for the table view
        private void AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // This is a crude method to limit the initial width of a column in case we expect it to become very wide...
            int columnindex = datatable.Columns[e.Column.Header.ToString()].Ordinal;
            if (fluxtable.Columns[columnindex].DataType == "string")
            {
                int maxlen = 0;
                foreach (var r in fluxtable.Records)
                {
                    int len = r.GetValueByIndex(columnindex).ToString().Length;
                    if (len > maxlen)
                        maxlen = len;
                }

                if (maxlen > 50)
                    e.Column.Width = new DataGridLength(300);
            }
        }
    }
}
