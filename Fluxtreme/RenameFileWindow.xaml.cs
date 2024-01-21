using System.Windows;
using System.Windows.Forms;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for RenameFileWindow.xaml
    /// </summary>
    public partial class RenameFileWindow : Window
    {
        public string Filename
        {
            get => namebox.Text;
            set
            {
                namebox.Text = value;
                namebox.SelectAll();
                namebox.Focus();
            }
        }

        public RenameFileWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
