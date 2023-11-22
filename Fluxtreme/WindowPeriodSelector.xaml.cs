using CodeImp.Fluxtreme.Tools;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for WindowPeriodSelector.xaml
    /// </summary>
    public partial class WindowPeriodSelector : UserControl
    {
        private TimeSpan period;

        public bool Automatic { get; private set; } = true;

        public TimeSpan Value
        {
            get => period;
            set
            {
                period = value;
                Automatic = true;
                periodbuttontext.Text = $"Auto ({period.ToShortString()})";
            }
        }

        public event EventHandler ValueChanged;

        public WindowPeriodSelector()
        {
            InitializeComponent();
        }

        private void Periodbutton_Click(object sender, RoutedEventArgs e)
        {
            periodmenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            periodmenu.PlacementTarget = periodbutton;
            periodmenu.IsOpen = true;
        }

        private void DisableContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void AutomaticPeriod_Click(object sender, RoutedEventArgs e)
        {
            Automatic = true;
            periodbuttontext.Text = "Auto";
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CustomPeriod_Click(object sender, RoutedEventArgs e)
        {
            WindowPeriodWindow wpw = new WindowPeriodWindow();
            wpw.SetPeriod(period);
            wpw.Owner = Window.GetWindow(this);
            bool? result = wpw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Automatic = false;
                period = wpw.GetPeriod();
                periodbuttontext.Text = $"Custom ({period.ToShortString()})";
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetWindowPeriod(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            period = TimeSpan.Parse(item.Tag.ToString(), CultureInfo.InvariantCulture);
            Automatic = false;
            periodbuttontext.Text = item.Header.ToString();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
