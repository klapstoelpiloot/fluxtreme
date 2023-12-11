using CodeImp.Fluxtreme.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CodeImp.Fluxtreme.Viewers
{
    /// <summary>
    /// Interaction logic for MultiTableView.xaml
    /// </summary>
    public partial class MultiTableView : UserControl
    {
        private List<FluxTableView> items;

        public MultiTableView()
        {
            InitializeComponent();
        }

        public void ShowTables(List<FluxTableView> results)
        {
            items = results;
            list.ItemsSource = items;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            FluxTableView tableview = dg.DataContext as FluxTableView;
            int columnindex = tableview.DataTable.Columns[e.PropertyName].Ordinal;

            // The first two columns are always "result" and "table" and we don't care.
            if (((columnindex == 0) && (e.PropertyName == "result")) ||
                ((columnindex == 1) && (e.PropertyName == "table")))
            {
                e.Cancel = true;
            }
            else
            {
                e.Column.Width = tableview.ColumnWidths[columnindex];
            }
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DataGrid && !e.Handled)
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dg)
            {
                // Don't allow row selection
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => dg.UnselectAll()));
            }
        }

        private void ExpandButton_Click(object sender, MouseButtonEventArgs e)
        {
            Image buttonimage = sender as Image;
            Button button = buttonimage.Parent as Button;
            FluxTableView tableview = button.DataContext as FluxTableView;
            tableview.ToggleExpand();

        }
    }
}
