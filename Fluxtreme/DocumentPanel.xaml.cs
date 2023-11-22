using CodeImp.Fluxtreme.Configuration;
using CodeImp.Fluxtreme.Data;
using CodeImp.Fluxtreme.Properties;
using System;
using System.Collections.Generic;
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

        public DocumentPanel()
        {
            InitializeComponent();

            datasourcebutton.Update();

            query = new QueryRunner();
            query.QueryError += Query_QueryError;
            query.DataReady += Query_DataReady;

            queryDelay = new System.Timers.Timer(200);
            queryDelay.AutoReset = false;
            queryDelay.Elapsed += QueryDelay_Elapsed;
        }

        public void Setup(string content = "")
        {
            editor.Setup();
            editor.Text = content;
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
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!disablesquerylines.Contains(i))
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

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            lock (syncobj)
            {
                querystring = editor.Text;
                disablesquerylines = new List<int>(editor.DisabledLines);
            }
            queryDelay.Stop();
            queryDelay.Start();
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
            datasourcebutton.Update();
        }

        private void DetermineAutoPeriod()
        {
            if (periodbutton.Automatic)
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

                Dispatcher.BeginInvoke(new Action(() => { periodbutton.Value = period; }));
            }
        }

        private void Period_ValueChanged(object sender, EventArgs e)
        {
            query.WindowPeriod = periodbutton.Value;
            RunQueryAsync();
        }

        private void TimeRange_ValueChanged(object sender, EventArgs e)
        {
            query.TimeRangeStart = timebutton.RangeStart;
            query.TimeRangeStop = timebutton.RangeStop;
            query.TimeRangeRecent = timebutton.RecentRange;
            RunQueryAsync();
        }

        private void DataSource_ValueChanged(object sender, EventArgs e)
        {
            query.SetDatasource(datasourcebutton.Value);
            RunQueryAsync();
        }
    }
}
