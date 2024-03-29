﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CodeImp.Fluxtreme.Configuration
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private List<UserControl> pages;

        public SettingsWindow()
        {
            InitializeComponent();

            // Make the list of settings pages
            pages = new List<UserControl>()
            {
                new DatasourceSettingsPage(),
                new EditorSettingsPage(),
                new MultiTableViewSettingsPage()
            };
            pageslist.ItemsSource = pages;
            pageslist.SelectedItem = pages[0];
        }

        public void SelectPage(Type pagetype)
        {
            UserControl page = pages.First(p => (p.GetType() == pagetype));
            pageslist.SelectedItem = page;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Window.SettingsChanged();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
