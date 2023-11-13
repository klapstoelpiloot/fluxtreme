using CodeImp.Fluxtreme.Properties;
using System.Windows;
using System.Windows.Controls;

namespace CodeImp.Fluxtreme.Configuration
{
    /// <summary>
    /// Interaction logic for DatasourceSettingsPage.xaml
    /// </summary>
    public partial class DatasourceSettingsPage : UserControl
    {
        public DatasourceSettingsPage()
        {
            InitializeComponent();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            DatasourceSettings ds = new DatasourceSettings();
            Settings.Default.Datasources.Add(ds);
            datasourceslist.SelectedItem = ds;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (datasourceslist.SelectedItem != null)
            {
                DatasourceSettings ds = datasourceslist.SelectedItem as DatasourceSettings;
                Settings.Default.Datasources.Remove(ds);
                datasourceslist.SelectedItem = null;
            }
        }

        public override string ToString()
        {
            return "Data sources";
        }
    }
}
