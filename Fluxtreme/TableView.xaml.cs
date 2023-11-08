using InfluxDB.Client.Core.Flux.Domain;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Fluxtreme
{
    /// <summary>
    /// Interaction logic for TableGrid.xaml
    /// </summary>
    public partial class TableView : UserControl
    {
        private FluxTable fluxtable;

        public TableView()
        {
            InitializeComponent();
        }

        public TableView(FluxTable fluxtable, TableExtraData extradata)
        {
            InitializeComponent();

            // Make columns
            for (int c = 0; c < fluxtable.Columns.Count; c++)
            {
                GridViewColumn col = new GridViewColumn();
                col.Header = fluxtable.Columns[c].Label;
                col.Width = extradata.ColumnWidths[c];
                col.DisplayMemberBinding = new Binding($"[{c}]");
                contentview.Columns.Add(col);
            }
            //contents.ItemsSource = fluxtable.Records;
            for (int r = 0; r < fluxtable.Records.Count; r++)
            {
                contents.Items.Add(new FluxRecordWrap(fluxtable.Records[r]));
            }

            // Make up table name
            foreach (FluxColumn c in fluxtable.GetGroupKey())
            {
                string colvalue = fluxtable.Records[0].Values[c.Label].ToString();

                title.Inlines.Add(new Run(c.Label + ":") { FontWeight = FontWeights.Bold });
                title.Inlines.Add(new Run(" " + colvalue) { FontWeight = FontWeights.Normal });
                title.Inlines.Add(new Run("   "));
            }
        }
    }
}
