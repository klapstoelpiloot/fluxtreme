﻿using CodeImp.Fluxtreme.Data;
using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace CodeImp.Fluxtreme.Viewers
{
    /// <summary>
    /// Interaction logic for TableGrid.xaml
    /// </summary>
    public partial class TableView : UserControl
    {
        private const int CollapsedRowCount = 3;
        private List<FluxRecordView> collapsedlist;
        private List<FluxRecordView> expandedlist;
        private bool expanded;

        public TableView()
        {
            InitializeComponent();
        }

        public TableView(FluxTableEx fluxtable)
        {
            InitializeComponent();

            // Show collapsed or expanded?
            expanded = fluxtable.Data.Records.Count <= CollapsedRowCount;
            if (expanded)
            {
                expandbutton.Visibility = Visibility.Hidden;
                fade.Visibility = Visibility.Collapsed;
            }

            // Make up table name
            if (fluxtable.Data.Records.Count > 0)
            {
                foreach (FluxColumn c in fluxtable.Data.GetGroupKey())
                {
                    string colvalue = fluxtable.Data.Records[0].Values[c.Label].ToString();
                    title.Inlines.Add(new Run(c.Label + ":") { FontWeight = FontWeights.Bold });
                    title.Inlines.Add(new Run(" " + colvalue) { FontWeight = FontWeights.Normal });
                    title.Inlines.Add(new Run("   "));
                }
            }
            else
            {
                foreach (FluxColumn c in fluxtable.Data.GetGroupKey())
                {
                    title.Inlines.Add(new Run(c.Label) { FontWeight = FontWeights.Bold });
                    title.Inlines.Add(new Run("   "));
                }
            }

            // Make columns
            for (int c = 2; c < fluxtable.Data.Columns.Count; c++)
            {
                GridViewColumn col = new GridViewColumn();
                col.Header = "_  " + fluxtable.Data.Columns[c].Label;
                col.Width = (int)fluxtable.ColumnWidths[c];
                col.DisplayMemberBinding = new Binding($"[{c}]");
                contentview.Columns.Add(col);
            }

            // Don't make this table smaller than the sum of its columns
            MinWidth = fluxtable.ColumnWidths.Sum();

            // Make rows. For the collapsedlist we just use a selection of items from the expandedlist.
            expandedlist = fluxtable.Data.Records.Select(r => new FluxRecordView(r)).ToList();
            collapsedlist = expandedlist.GetRange(0, Math.Min(CollapsedRowCount, expandedlist.Count));

            // Show contents
            contents.ItemsSource = expanded ? expandedlist : collapsedlist;
        }

        private void expandbutton_Click(object sender, RoutedEventArgs e)
        {
            if (expanded)
            {
                expanded = false;
                contents.ItemsSource = collapsedlist;
                expandbutton.Content = FindResource("expandimage");
                fade.Visibility = Visibility.Visible;
            }
            else
            {
                expanded = true;
                contents.ItemsSource = expandedlist;
                expandbutton.Content = FindResource("collapseimage");
                fade.Visibility = Visibility.Collapsed;
            }
        }

        private void contents_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is ListBox && !e.Handled)
            {
                // Don't scroll this ListView, but instead pass on the scroll event to the parent control(s).
                e.Handled = true;
                MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                UIElement parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}