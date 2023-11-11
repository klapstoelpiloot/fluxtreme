using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Fluxtreme
{
    public partial class DocumentPanel : UserControl
    {
        private static readonly Brush StatusErrorBackground = new SolidColorBrush(Color.FromRgb(200, 0, 0));

        private System.Timers.Timer queryDelay;
        private QueryRunner query;
        private string querystring;

        public DocumentPanel()
        {
            InitializeComponent();

            // TODO: Dispose query
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
            if (!query.IsRunning)
            {
                Dispatcher.BeginInvoke(new Action(() => ShowQueryInProgress()));
                query.Run(querystring);
            }
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            querystring = editor.Text;
            queryDelay.Stop();
            queryDelay.Start();
        }
    }
}
