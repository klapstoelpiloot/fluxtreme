using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows;

namespace Fluxtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MonacoHostBridge bridge = new MonacoHostBridge();
        private object querySyncObject = new object();
        private System.Timers.Timer queryDelay;
        private string query;

        public MainWindow()
        {
            InitializeComponent();

            bridge.OnTextChanged += Bridge_OnTextChanged;

            queryDelay = new System.Timers.Timer(100);
            queryDelay.AutoReset = false;
            queryDelay.Elapsed += QueryDelay_Elapsed;
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            await queryeditor.EnsureCoreWebView2Async();
            queryeditor.CoreWebView2.AddHostObjectToScript("bridge", bridge);
            queryeditor.Source = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Monaco\index.html"));
        }

        public void ShowQueryError(string message, int fromLine = -1, int fromCol = -1, int toLine = -1, int toCol = -1)
        {
            queryerror.Content = message;
            queryerror.ToolTip = message;
            queryerror.Visibility = Visibility.Visible;
            if ((fromLine > -1) && (fromCol > -1) && (toLine > -1) && (toCol > -1))
            {
                queryeditor.ExecuteScriptAsync($"DisplayErrorMarker({fromLine}, {toLine}, {fromCol}, {toCol}, \"{HttpUtility.JavaScriptStringEncode(message)}\");");
            }
            else
            {
                queryeditor.ExecuteScriptAsync($"ClearErrorMarker();");
            }
        }

        public void HideQueryError()
        {
            queryerror.Visibility = Visibility.Hidden;
            queryeditor.ExecuteScriptAsync($"ClearErrorMarker();");
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

        private void Bridge_OnTextChanged(string text)
        {
            query = text;
            queryDelay.Stop();
            queryDelay.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.Default.Save();
        }
    }
}
