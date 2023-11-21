using System;
using System.Globalization;
using System.Windows;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for WindowPeriodWindow.xaml
    /// </summary>
    public partial class WindowPeriodWindow : Window
    {
        public WindowPeriodWindow()
        {
            InitializeComponent();
        }

        public void SetPeriod(TimeSpan t)
        {
            daysbox.Text = t.Days.ToString(CultureInfo.InvariantCulture);
            hoursbox.Text = t.Hours.ToString(CultureInfo.InvariantCulture);
            minutesbox.Text = t.Minutes.ToString(CultureInfo.InvariantCulture);
            secondsbox.Text = t.Seconds.ToString(CultureInfo.InvariantCulture);
            millisecondsbox.Text = t.Milliseconds.ToString(CultureInfo.InvariantCulture);
            daysbox.Focus();
        }

        public TimeSpan GetPeriod()
        {
            int d = int.Parse(daysbox.Text, CultureInfo.InvariantCulture);
            int h = int.Parse(hoursbox.Text, CultureInfo.InvariantCulture);
            int m = int.Parse(minutesbox.Text, CultureInfo.InvariantCulture);
            int s = int.Parse(secondsbox.Text, CultureInfo.InvariantCulture);
            int ms = int.Parse(millisecondsbox.Text, CultureInfo.InvariantCulture);
            return new TimeSpan(d, h, m, s, ms);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(daysbox.Text)) daysbox.Text = "0";
            if (string.IsNullOrWhiteSpace(hoursbox.Text)) hoursbox.Text = "0";
            if (string.IsNullOrWhiteSpace(minutesbox.Text)) minutesbox.Text = "0";
            if (string.IsNullOrWhiteSpace(secondsbox.Text)) secondsbox.Text = "0";
            if (string.IsNullOrWhiteSpace(millisecondsbox.Text)) millisecondsbox.Text = "0";

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
