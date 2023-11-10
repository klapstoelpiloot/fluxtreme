using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Fluxtreme
{
    /// <summary>
    /// Interaction logic for TableGrid.xaml
    /// </summary>
    public partial class TableView : UserControl
    {
        private const int CollapsedRowCount = 3;
        private FluxTable fluxtable;

        public TableView()
        {
            InitializeComponent();
        }

        public TableView(FluxTable fluxtable, TableExtraData extradata)
        {
            InitializeComponent();

            this.fluxtable = fluxtable;

            bool expanded = fluxtable.Records.Count <= CollapsedRowCount;
            if (expanded)
            {
                expandbutton.Visibility = Visibility.Hidden;
                fade.Visibility = Visibility.Collapsed;
            }

            // Make up table name
            if (fluxtable.Records.Count > 0)
            {
                foreach (FluxColumn c in fluxtable.GetGroupKey())
                {
                    string colvalue = fluxtable.Records[0].Values[c.Label].ToString();
                    title.Inlines.Add(new Run(c.Label + ":") { FontWeight = FontWeights.Bold });
                    title.Inlines.Add(new Run(" " + colvalue) { FontWeight = FontWeights.Normal });
                    title.Inlines.Add(new Run("   "));
                }
            }
            else
            {
                foreach (FluxColumn c in fluxtable.GetGroupKey())
                {
                    title.Inlines.Add(new Run(c.Label) { FontWeight = FontWeights.Bold });
                    title.Inlines.Add(new Run("   "));
                }
            }

            // Make columns
            for (int c = 2; c < fluxtable.Columns.Count; c++)
            {
                GridViewColumn col = new GridViewColumn();
                col.Header = "_  " + fluxtable.Columns[c].Label;
                col.Width = (int)extradata.ColumnWidths[c];
                col.DisplayMemberBinding = new Binding($"[{c}]");
                contentview.Columns.Add(col);
            }

            // Make rows
            for (int r = 0; r < Math.Min(fluxtable.Records.Count, CollapsedRowCount); r++)
            {
                contents.Items.Add(new FluxRecordWrap(fluxtable.Records[r]));
            }
        }

        private void expandbutton_Click(object sender, RoutedEventArgs e)
        {
            // Add the remaining rows
            for (int r = CollapsedRowCount; r < fluxtable.Records.Count; r++)
            {
                contents.Items.Add(new FluxRecordWrap(fluxtable.Records[r]));
            }

            expandbutton.Visibility = Visibility.Hidden;
            fade.Visibility = Visibility.Collapsed;
        }

        private void contents_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is ListBox && !e.Handled)
            {
                // Don't scroll this ListView, but instead pass on the scroll event to the parent control(s).
                e.Handled = true;
                MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                UIElement parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
