using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void Window_Initialized(object sender, EventArgs e)
        {
            using (InfluxDBClient client = new InfluxDBClient($"http://{AppSettings.Default.DatabaseAddress}/", AppSettings.Default.DatabaseAccessToken))
            {
                tableslist.Children.Clear();

                string q = $"from(bucket: \"events\")" +
                    $"|> range(start: 2023-11-03T00:00:00Z, stop: 2023-11-04T00:00:00Z)" +
                    $"|> filter(fn: (r) => r[\"code\"] =~ /[IE]_SPE_03F2/)" +
                    $"|> filter(fn: (r) => r[\"_field\"] == \"p-holder\" or r[\"_field\"] == \"p-mafm\" or r[\"_field\"] == \"p-sniffle_target\" or r[\"_field\"] == \"description\")" +
                    $"|> filter(fn: (r) => r[\"ssindex\"] == \"0\")";
                QueryApiSync reader = client.GetQueryApiSync();
                List<FluxTable> tables = reader.QuerySync(q, AppSettings.Default.DatabaseOrganizationID);
                foreach(FluxTable t in tables)
                {
                    TableGrid control = new TableGrid(t);
                    tableslist.Children.Add(control);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.Default.Save();
        }
    }
}
