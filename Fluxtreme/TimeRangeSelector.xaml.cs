using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for TimeRangeSelector.xaml
    /// </summary>
    public partial class TimeRangeSelector : UserControl
    {
        public DateTime RangeStart { get; private set; }
        public DateTime RangeStop { get; private set; }
        public TimeSpan RecentRange { get; private set; }

        public event EventHandler ValueChanged;

        public TimeRangeSelector()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            timemenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            timemenu.PlacementTarget = timebutton;
            timemenu.IsOpen = true;
        }

        private void DisableContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void CustomTimeRange_Click(object sender, RoutedEventArgs e)
        {
            TimeRangeWindow trw = new TimeRangeWindow();
            if (RecentRange != TimeSpan.Zero)
            {
                trw.SetDates(DateTime.Now - RecentRange, DateTime.Now);
            }
            else
            {
                trw.SetDates(RangeStart, RangeStop);
            }
            trw.Owner = Window.GetWindow(this);
            bool? result = trw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                RangeStart = trw.GetFromDate();
                RangeStop = trw.GetToDate();
                RecentRange = TimeSpan.Zero;
                timebuttontext.Text = RangeStart.ToShortDateString() + " " + RangeStart.ToShortTimeString() + " - " + 
                    RangeStop.ToShortDateString() + " " + RangeStop.ToShortTimeString();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetRecentTimeRange(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            TimeSpan t = TimeSpan.Parse(item.Tag.ToString(), CultureInfo.InvariantCulture);
            RecentRange = t;
            timebuttontext.Text = item.Header.ToString();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
