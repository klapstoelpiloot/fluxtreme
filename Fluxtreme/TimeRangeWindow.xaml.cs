using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for TimeRangeWindow.xaml
    /// </summary>
    public partial class TimeRangeWindow : Window
    {
        private static readonly Regex NumericInputRegex = new Regex("^[0-9]{1,2}$");

        public TimeRangeWindow()
        {
            InitializeComponent();
        }

        public void SetDates(DateTime from, DateTime to)
        {
            fromdate.SelectedDate = from.Date;
            fromhour.Text = from.TimeOfDay.Hours.ToString(CultureInfo.InvariantCulture);
            fromminute.Text = from.TimeOfDay.Minutes.ToString(CultureInfo.InvariantCulture);
            todate.SelectedDate = to.Date;
            tohour.Text = to.TimeOfDay.Hours.ToString(CultureInfo.InvariantCulture);
            tominute.Text = to.TimeOfDay.Minutes.ToString(CultureInfo.InvariantCulture);
        }

        public DateTime GetFromDate()
        {
            DateTime d = fromdate.SelectedDate.Value;
            TimeSpan t = new TimeSpan(int.Parse(fromhour.Text, CultureInfo.InvariantCulture), int.Parse(fromminute.Text, CultureInfo.InvariantCulture), 0);
            return d + t;
        }

        public DateTime GetToDate()
        {
            DateTime d = todate.SelectedDate.Value;
            TimeSpan t = new TimeSpan(int.Parse(tohour.Text, CultureInfo.InvariantCulture), int.Parse(tominute.Text, CultureInfo.InvariantCulture), 0);
            return d + t;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(fromhour.Text)) fromhour.Text = "0";
            if(string.IsNullOrWhiteSpace(fromminute.Text)) fromminute.Text = "0";
            if(string.IsNullOrWhiteSpace(tohour.Text)) tohour.Text = "0";
            if(string.IsNullOrWhiteSpace(tominute.Text)) tominute.Text = "0";

            // Check that the input is valid


            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
