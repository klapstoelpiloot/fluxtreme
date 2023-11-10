using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Fluxtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private object querySyncObject = new object();
        private System.Timers.Timer queryDelay;
        private string query;

        public MainWindow()
        {
            InitializeComponent();

            queryDelay = new System.Timers.Timer(100);
            queryDelay.AutoReset = false;
            queryDelay.Elapsed += QueryDelay_Elapsed;

            editor.Text = "from(bucket: \"events\")\r\n" +
					"\t|> range(start: 2023-11-03T00:00:00Z, stop: 2023-11-04T00:00:00Z)\r\n" +
					"\t|> filter(fn: (r) => r[\"code\"] =~ /[IE]_SPE_03F2/)\r\n" +
					"\t|> filter(fn: (r) => r[\"_field\"] == \"p-holder\" or r[\"_field\"] == \"p-mafm\" or r[\"_field\"] == \"p-sniffle_target\" or r[\"_field\"] == \"description\")\r\n" +
					"\t|> filter(fn: (r) => r[\"ssindex\"] == \"0\")\r\n";
        }

        public void ShowQueryError(string message, int fromLine = -1, int fromCol = -1, int toLine = -1, int toCol = -1)
        {
            queryerror.Content = message;
            queryerror.ToolTip = message;
            queryerror.Visibility = Visibility.Visible;
        }

        public void HideQueryError()
        {
            queryerror.Visibility = Visibility.Hidden;
        }

        public void ShowQueryResults(List<FluxTable> tables, List<TableExtraData> extradata)
        {
            List<TableView> grids = new List<TableView>();
            for(int i = 0; i < tables.Count; i++)
            {
                TableView t = new TableView(tables[i], extradata[i]);
                grids.Add(t);
            }
            tableslist.Children.Clear();
            grids.ForEach(t => tableslist.Children.Add(t));
        }

        private void QueryDelay_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<FluxTable> tables;

            if (Monitor.TryEnter(querySyncObject, 1))
            {
                try
                {
                    try
                    {
                        // Perform the query
                        using (InfluxDBClient client = new InfluxDBClient($"http://{AppSettings.Default.DatabaseAddress}/", AppSettings.Default.DatabaseAccessToken))
                        {
                            QueryApiSync reader = client.GetQueryApiSync();
                            tables = reader.QuerySync(query, AppSettings.Default.DatabaseOrganizationID);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Something is wrong
                        const string CompilerMessage = "compilation failed: ";
                        Regex CompileErrorRegex = new Regex("^error @(\\d+):(\\d+)-(\\d+):(\\d+)");
                        string msg = ex.Message;
                        if (msg.StartsWith("compilation failed:"))
                        {
                            // Parse as compile errors
                            msg = msg.Substring(CompilerMessage.Length);
                            List<string> errors = new List<string>();
                            foreach (string er in msg.Split('\n'))
                            {
                                if (!string.IsNullOrWhiteSpace(er))
                                {
                                    errors.Add(er.Trim());
                                }
                            }
                            if (errors.Count > 0)
                            {
                                msg = string.Join("\n", errors);
                                Match coordinates = CompileErrorRegex.Match(errors[0]);
                                if ((coordinates.Captures.Count > 0) && (coordinates.Groups.Count > 4))
                                {
                                    int lineFrom = int.Parse(coordinates.Groups[1].Value);
                                    int colFrom = int.Parse(coordinates.Groups[2].Value);
                                    int lineTo = int.Parse(coordinates.Groups[3].Value);
                                    int colTo = int.Parse(coordinates.Groups[4].Value);
                                    Dispatcher.BeginInvoke(new Action(() => ShowQueryError(msg, lineFrom, colFrom, lineTo, colTo)));
                                }
                                else
                                {
                                    Dispatcher.BeginInvoke(new Action(() => ShowQueryError(msg)));
                                }
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(new Action(() => ShowQueryError(msg)));
                            }
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() => ShowQueryError(msg)));
                        }
                        return;
                    }

                    // Calculate extra data for all tables received
                    List<TableExtraData> extradata = tables.Select(t => TableExtraData.CalculateFor(t)).ToList();

                    // Show the result
                    Dispatcher.BeginInvoke(new Action(() => HideQueryError()));
                    Dispatcher.BeginInvoke(new Action(() => ShowQueryResults(tables, extradata)));
                }
                finally
                {
                    Monitor.Exit(querySyncObject);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.Default.Save();
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            query = editor.Text;
            queryDelay.Stop();
            queryDelay.Start();
        }
    }
}
