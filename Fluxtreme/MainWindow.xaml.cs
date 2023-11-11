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
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace Fluxtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.Default.Save();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            // Something to test quickly
            TabItem tab = new TabItem();
            tab.Header = "Query 1";
            DocumentPanel dc = new DocumentPanel();
            dc.Setup("from(bucket: \"events\")\r\n" +
					"\t|> range(start: 2023-11-03T00:00:00Z, stop: 2023-11-04T00:00:00Z)\r\n" +
					"\t|> filter(fn: (r) => r[\"code\"] =~ /[IE]_SPE_03F2/)\r\n" +
					"\t|> filter(fn: (r) => r[\"_field\"] == \"p-holder\" or r[\"_field\"] == \"p-mafm\" or r[\"_field\"] == \"p-sniffle_target\" or r[\"_field\"] == \"description\")\r\n" +
					"\t|> filter(fn: (r) => r[\"ssindex\"] == \"0\")\r\n");
            tab.Content = dc;
            tabs.Items.Add(tab);
        }
    }
}
