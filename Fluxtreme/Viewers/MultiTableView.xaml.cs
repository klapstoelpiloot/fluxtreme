using CodeImp.Fluxtreme.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CodeImp.Fluxtreme.Viewers
{
    /// <summary>
    /// Interaction logic for MultiTableView.xaml
    /// </summary>
    public partial class MultiTableView : UserControl
    {
        private const int NumTablesPerIteration = 3;

        private Queue<TableView> addqueue = new Queue<TableView>();

        public MultiTableView()
        {
            InitializeComponent();
        }

        public void ShowTables(List<FluxTableEx> results)
        {
            lock (addqueue)
            {
                addqueue.Clear();
                for (int i = 0; i < results.Count; i++)
                {
                    TableView t = new TableView(results[i]);
                    addqueue.Enqueue(t);
                }
                tableslist.ColumnDefinitions.Clear();
                tableslist.Children.Clear();
            }

            Dispatcher.BeginInvoke(new Action(NextDispatchJob));
        }

        private void NextDispatchJob()
        {
            lock (addqueue)
            {
                for(int i = 0; i < NumTablesPerIteration; i++)
                {
                    if (addqueue.Count > 0)
                    {
                        TableView t = addqueue.Dequeue();
                        int tindex = tableslist.Children.Count;
                        Grid.SetRow(t, tindex);
                        if(tableslist.RowDefinitions.Count <= tindex)
                        {
                            tableslist.RowDefinitions.Add(new RowDefinition());
                        }
                        tableslist.Children.Add(t);
                    }
                }

                if (addqueue.Count > 0)
                {
                    Dispatcher.BeginInvoke(new Action(NextDispatchJob), DispatcherPriority.Background);
                }
            }
        }
    }
}
