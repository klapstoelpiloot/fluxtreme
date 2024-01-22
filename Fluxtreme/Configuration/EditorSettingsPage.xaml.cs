using System.Windows.Controls;

namespace CodeImp.Fluxtreme.Configuration
{
    /// <summary>
    /// Interaction logic for EditorSettingsPage.xaml
    /// </summary>
    public partial class EditorSettingsPage : UserControl
    {
        public EditorSettingsPage()
        {
            InitializeComponent();
        }

        public override string ToString()
        {
            return "Editor";
        }
    }
}
