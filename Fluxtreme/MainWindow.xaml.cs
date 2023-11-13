using CodeImp.Fluxtreme.Configuration;
using CodeImp.Fluxtreme.Properties;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            // Something to test quickly
            TabItem tab = new TabItem();
            tab.Header = "Query 1";
            DocumentPanel dp = new DocumentPanel();
            dp.Setup("from(bucket: \"events\")\r\n" +
					"\t|> range(start: v.timeRangeStart, stop: v.timeRangeStop)\r\n" +
					"\t|> filter(fn: (r) => r[\"code\"] =~ /[IE]_SPE_03F2/)\r\n" +
					"\t|> filter(fn: (r) => r[\"_field\"] == \"p-holder\" or r[\"_field\"] == \"p-mafm\" or r[\"_field\"] == \"p-sniffle_target\" or r[\"_field\"] == \"description\")\r\n" +
					"\t|> filter(fn: (r) => r[\"ssindex\"] == \"0\")\r\n");
            tab.Content = dp;
            tabs.Items.Add(tab);
        }

        public void SettingsChanged()
        {
            foreach(TabItem tab in tabs.Items)
            {
                IDocumentPanel dp = tab.Content as IDocumentPanel;
                dp.SettingsChanged();
            }
        }
    }
}
