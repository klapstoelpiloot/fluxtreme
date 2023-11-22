using CodeImp.Fluxtreme.Configuration;
using CodeImp.Fluxtreme.Data;
using CodeImp.Fluxtreme.Properties;
using CodeImp.Fluxtreme.Viewers;
using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
        private List<int> disablesquerylines;
        private object syncobj = new object();
        private bool autoperiod = true;

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
            foreach (Control item in datasourcemenu.Items)
                prevmenuitems.Add(item);

            // Remove previous menu items
            foreach (Control item in prevmenuitems)
            {
                if (item is Separator)
                {
                    continue;
                }
                else if (item is MenuItem menuitem)
                {
                    if (menuitem.Tag != null)
                    {
                        menuitem.Click -= SetDatasource;
                        datasourcemenu.Items.Remove(menuitem);
                    }
                }
            }

            // Add new items
            if (Settings.Default.Datasources != null)
            {
                foreach (DatasourceSettings ds in Settings.Default.Datasources)
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

        public void ShowErrorStatus(string message, TextRange range)
        {
            editor.ShowErrorIndicator(range);
            statuslabel.Text = message;
            statusbar.ToolTip = message;
            statusbar.Background = StatusErrorBackground;
            statuslabel.Foreground = (Brush)FindResource("Button.Static.Foreground");
            progressbar.Visibility = Visibility.Hidden;
        }

        public void ShowNormalStatus(string message)
        {
            editor.ClearErrorIndiciator();
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

        private void Query_DataReady(List<FluxTableEx> result, TimeSpan duration)
        {
            int recordcount = 0;
            result.ForEach(t => recordcount += t.Data.Records.Count);
            Dispatcher.BeginInvoke(new Action(() => ShowNormalStatus($"Query took {duration.TotalSeconds:0.00} seconds. Tables: {result.Count} Records: {recordcount}")));
            Dispatcher.BeginInvoke(new Action(() => tableview.ShowTables(result)));
        }

        private void Query_QueryError(List<string> messages, List<TextRange> ranges)
        {
            Dispatcher.BeginInvoke(new Action(() => ShowErrorStatus(messages.First(), ranges.First())));
        }

        private void QueryDelay_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RunQuery();
        }

        private void RunQuery()
        {
            if (!query.IsRunning)
            {
                DetermineAutoPeriod();
                Dispatcher.BeginInvoke(new Action(() => ShowQueryInProgress()));

                StringBuilder finalquery = new StringBuilder();
                lock (syncobj)
                {
                    string[] lines = querystring.Split('\n');
                    for(int i = 0; i < lines.Length; i++)
                    {
                        if(!disablesquerylines.Contains(i))
                        {
                            finalquery.AppendLine(lines[i].Trim());
                        }
                    }
                }
                query.Run(finalquery.ToString());
            }
        }

        private void RunQueryAsync()
        {
            if (IsLoaded)
            {
                Task task = new Task(RunQuery);
                task.Start();
            }
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            lock (syncobj)
            {
                querystring = editor.Text;
                disablesquerylines = new List<int>(editor.DisabledLines);
            }
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
            TimeRangeWindow trw = new TimeRangeWindow();
            if (query.TimeRangeRecent != TimeSpan.Zero)
            {
                trw.SetDates(DateTime.Now - query.TimeRangeRecent, DateTime.Now);
            }
            else
            {
                trw.SetDates(query.TimeRangeStart, query.TimeRangeStop);
            }
            trw.Owner = Window.GetWindow(this);
            bool? result = trw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                query.TimeRangeStart = trw.GetFromDate();
                query.TimeRangeStop = trw.GetToDate();
                query.TimeRangeRecent = TimeSpan.Zero;
                timebuttontext.Text = query.TimeRangeStart.ToShortDateString() + " " + query.TimeRangeStart.ToShortTimeString() + " - " +
                    query.TimeRangeStop.ToShortDateString() + " " + query.TimeRangeStop.ToShortTimeString();
                RunQueryAsync();
            }
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

        private void Periodbutton_Click(object sender, RoutedEventArgs e)
        {
            periodmenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            periodmenu.PlacementTarget = periodbutton;
            periodmenu.IsOpen = true;
        }

        private void AutomaticPeriod_Click(object sender, RoutedEventArgs e)
        {
            autoperiod = true;
            periodbuttontext.Text = "Auto";
            RunQueryAsync();
        }

        private void CustomPeriod_Click(object sender, RoutedEventArgs e)
        {
            WindowPeriodWindow wpw = new WindowPeriodWindow();
            wpw.SetPeriod(query.WindowPeriod);
            wpw.Owner = Window.GetWindow(this);
            bool? result = wpw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                autoperiod = false;
                query.WindowPeriod = wpw.GetPeriod();
                periodbuttontext.Text = $"Custom ({TimeSpanToShortString(query.WindowPeriod)})";
                RunQueryAsync();
            }
        }

        private void SetWindowPeriod(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            TimeSpan t = TimeSpan.Parse(item.Tag.ToString(), CultureInfo.InvariantCulture);
            query.WindowPeriod = t;
            autoperiod = false;
            periodbuttontext.Text = item.Header.ToString();
            RunQueryAsync();
        }

        private void DetermineAutoPeriod()
        {
            if (autoperiod)
            {
                TimeSpan range;
                if (query.TimeRangeRecent == TimeSpan.Zero)
                {
                    range = query.TimeRangeStop - query.TimeRangeStart;
                }
                else
                {
                    range = query.TimeRangeRecent;
                }

                // Divide the time range over the width of the window.
                // By this logic, the window period is chosen to provide just enough data for every pixel on the screen.
                TimeSpan period = new TimeSpan(range.Ticks / (long)ActualWidth);
                query.WindowPeriod = period;

                Dispatcher.BeginInvoke(new Action(() => { periodbuttontext.Text = $"Auto ({TimeSpanToShortString(period)})"; }));
            }
        }

        private string TimeSpanToShortString(TimeSpan t)
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

        private void DisableContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}
