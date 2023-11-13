using CodeImp.Fluxtreme.Configuration;
using CodeImp.Fluxtreme.Data;
using CodeImp.Fluxtreme.Properties;
using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeImp.Fluxtreme
{
    public partial class DocumentPanel : UserControl, IDocumentPanel
    {
        private static readonly Brush StatusErrorBackground = new SolidColorBrush(Color.FromRgb(200, 0, 0));

        private System.Timers.Timer queryDelay;
        private QueryRunner query;
        private string querystring;

        public DocumentPanel()
        {
            InitializeComponent();

            UpdateDatasources();

            query = new QueryRunner();
            query.QueryError += Query_QueryError;
            query.DataReady += Query_DataReady;

            queryDelay = new System.Timers.Timer(200);
            queryDelay.AutoReset = false;
            queryDelay.Elapsed += QueryDelay_Elapsed;
            
            SetRecentTimeRange(defaulttimerange, EventArgs.Empty);
        }

        public void Setup(string content = "")
        {
            editor.Setup();
            editor.Text = content;
        }

        public void UpdateDatasources()
        {
            // Copy list of objects so that we can iterate over
            // this list and remove the items from the control
            List<Control> prevmenuitems = new List<Control>();
            foreach(Control item in datasourcemenu.Items)
                prevmenuitems.Add(item);

            // Remove previous menu items
            foreach(Control item in prevmenuitems)
            {
                if (item is Separator)
                {
                    continue;
                }
                else if(item is MenuItem menuitem)
                {
                    if(menuitem.Tag != null)
                    {
                        menuitem.Click -= SetDatasource;
                        datasourcemenu.Items.Remove(menuitem);
                    }
                }
            }

            // Add new items
            if(Settings.Default.Datasources != null)
            {
                foreach(DatasourceSettings ds in Settings.Default.Datasources)
                {
                    MenuItem item = new MenuItem();
                    item.Header = ds.Name;
                    item.Tag = ds;
                    item.Click += SetDatasource;
                    datasourcemenu.Items.Insert(0, item);
                }
            }

            // If the selected datasource is not in the list (anymore) then deselect it
            // We don't want to automatically select the next (or another) datasource as
            // it may accedentially query another database.
            DatasourceSettings current = datasourcebutton.DataContext as DatasourceSettings;
            if ((current != null) && !Settings.Default.Datasources.Contains(current))
                SetDatasource(null);
        }

        public void ShowErrorStatus(string message)
        {
            statuslabel.Text = message;
            statusbar.ToolTip = message;
            statusbar.Background = StatusErrorBackground;
            statuslabel.Foreground = (Brush)FindResource("Button.Static.Foreground");
            progressbar.Visibility = Visibility.Hidden;
        }

        public void ShowNormalStatus(string message)
        {
            statuslabel.Text = message;
            statusbar.ToolTip = message;
            statusbar.Background = (Brush)FindResource("PanelBackground4");
            statuslabel.Foreground = (Brush)FindResource("StatusBar.NormalText");
            progressbar.Visibility = Visibility.Hidden;
        }

        public void ShowQueryInProgress()
        {
            progressbar.IsIndeterminate = false;
            progressbar.Visibility = Visibility.Visible;
            progressbar.IsIndeterminate = true;
        }

        public void SetDatasource(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            SetDatasource(item.Tag as DatasourceSettings);
        }

        public void SetDatasource(DatasourceSettings ds)
        {
            datasourcebutton.DataContext = ds;
            query.SetDatasource(ds);
            RunQueryAsync();
        }

        public void SetRecentTimeRange(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            TimeSpan t = TimeSpan.Parse(item.Tag.ToString(), CultureInfo.InvariantCulture);
            query.TimeRangeRecent = t;
            timebuttontext.Text = item.Header.ToString();
            RunQueryAsync();
        }

        public void ShowQueryResults(List<FluxTable> tables, List<TableExtraData> extradata)
        {
            List<TableView> grids = new List<TableView>();
            for (int i = 0; i < tables.Count; i++)
            {
                TableView t = new TableView(tables[i], extradata[i]);
                grids.Add(t);
            }
            tableslist.Children.Clear();
            grids.ForEach(t => tableslist.Children.Add(t));
        }

        private void Query_DataReady(List<FluxTable> tables, List<TableExtraData> extradata, TimeSpan duration)
        {
            Dispatcher.BeginInvoke(new Action(() => ShowNormalStatus($"Query took {duration.TotalSeconds:0.00} seconds")));
            Dispatcher.BeginInvoke(new Action(() => ShowQueryResults(tables, extradata)));
        }

        private void Query_QueryError(List<string> messages)
        {
            Dispatcher.BeginInvoke(new Action(() => ShowErrorStatus(messages.First())));
        }

        private void QueryDelay_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RunQuery();
        }

        private void RunQuery()
        {
            if (!query.IsRunning && (querystring != null))
            {
                Dispatcher.BeginInvoke(new Action(() => ShowQueryInProgress()));
                query.Run(querystring);
            }
        }

        private void RunQueryAsync()
        {
            Task task = new Task(RunQuery);
            task.Start();
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            querystring = editor.Text;
            queryDelay.Stop();
            queryDelay.Start();
        }

        private void timebutton_Click(object sender, RoutedEventArgs e)
        {
            timemenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            timemenu.PlacementTarget = timebutton;
            timemenu.IsOpen = true;
        }

        private void CustomTimeRange_Click(object sender, RoutedEventArgs e)
        {

        }

        private void datasourcebutton_Click(object sender, RoutedEventArgs e)
        {
            datasourcemenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            datasourcemenu.PlacementTarget = datasourcebutton;
            datasourcemenu.IsOpen = true;
        }

        private void ConfigureDatasources_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Owner = Window.GetWindow(this);
            sw.SelectPage(typeof(DatasourceSettingsPage));
            sw.ShowDialog();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (query != null)
            {
                query.Dispose();
                query = null;
            }
        }

        public void SettingsChanged()
        {
            UpdateDatasources();

        }
    }
}
