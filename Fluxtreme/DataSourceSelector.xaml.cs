using CodeImp.Fluxtreme.Configuration;
using CodeImp.Fluxtreme.Properties;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for DataSourceSelector.xaml
    /// </summary>
    public partial class DataSourceSelector : UserControl
    {
        private DatasourceSettings selected = null;

        public DatasourceSettings Value
        {
            get => selected;
            set
            {
                selected = value;
                datasourcebutton.DataContext = selected;
            }
        }

        public event EventHandler ValueChanged;

        public DataSourceSelector()
        {
            InitializeComponent();
        }

        public void CopyTo(DataSourceSelector other)
        {
            other.selected = this.selected;
            other.datasourcebutton.DataContext = other.selected;
            other.ValueChanged?.Invoke(other, EventArgs.Empty);
        }

        public void Update()
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
            if ((selected != null) && !Settings.Default.Datasources.Contains(selected))
            {
                selected = null;
                datasourcebutton.DataContext = null;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datasourcemenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            datasourcemenu.PlacementTarget = datasourcebutton;
            datasourcemenu.IsOpen = true;
        }

        private void SetDatasource(object sender, EventArgs e)
        {
            if (sender is MenuItem menuitem)
            {
                selected = menuitem.Tag as DatasourceSettings;
                datasourcebutton.DataContext = selected;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DisableContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void ConfigureDatasources_Click(object sender, RoutedEventArgs e)
        {
            App.Window.ShowSettings(typeof(DatasourceSettingsPage));
            Update();
        }
    }
}
