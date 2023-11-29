using CodeImp.Fluxtreme.Configuration;
using CodeImp.Fluxtreme.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeImp.Fluxtreme
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        private const string NewFileName = "Query ";

        public static RoutedCommand NewFileCommand { get; private set; } = new RoutedCommand();
        public static RoutedCommand OpenFileCommand { get; private set; } = new RoutedCommand();
        public static RoutedCommand DuplicateFileCommand { get; private set; } = new RoutedCommand();
        public static RoutedCommand ShowSettingsCommand { get; private set; } = new RoutedCommand();
        public static RoutedCommand SaveFileCommand { get; private set; } = new RoutedCommand();
        public static RoutedCommand SaveFileAsCommand { get; private set; } = new RoutedCommand();
        public static RoutedCommand ExitCommand { get; private set; } = new RoutedCommand();

        public DocumentTab CurrentTab => tabs.SelectedItem as DocumentTab;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Shortcut keys for commands
            NewFileCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            OpenFileCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            SaveFileCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            DuplicateFileCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            // Something to test quickly
            DocumentTab tab = DocumentTab.New(MakeNewFileName(NewFileName));
            tab.Panel.Query = "from(bucket: \"events\")\r\n" +
                    "\t|> range(start: v.timeRangeStart, stop: v.timeRangeStop)\r\n" +
                    "\t// The lines above are very common. The lines below are very specific.\r\n" +
                    "\t|> filter(fn: (r) => r[\"code\"] =~ /[IE]_SPE_03F2/)\r\n" +
                    "\t|> filter(fn: (r) => r[\"_field\"] == \"p-holder\" or r[\"_field\"] == \"p-mafm\" or r[\"_field\"] == \"p-sniffle_target\" or r[\"_field\"] == \"description\")\r\n" +
                    "\t|> filter(fn: (r) => r[\"ssindex\"] == \"0\")\r\n";
            tabs.Items.Add(tab);
        }

        public void SettingsChanged()
        {
            foreach (TabItem tab in tabs.Items)
            {
                IDocumentPanel dp = tab.Content as IDocumentPanel;
                dp.SettingsChanged();
            }
        }

        public void NewFile(object sender, ExecutedRoutedEventArgs e)
        {
            DocumentTab newtab = DocumentTab.New(MakeNewFileName(NewFileName));
            tabs.Items.Add(newtab);
            tabs.SelectedItem = newtab;
        }

        private List<DocumentTab> GetTabs()
        {
            List<DocumentTab> tabitems = new List<DocumentTab>(tabs.Items.Count);
            for (int i = 0; i < tabs.Items.Count; i++)
            {
                tabitems.Add(tabs.Items[i] as DocumentTab);
            }
            return tabitems;
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Show a file browser to open a file
            openFileDialog.Title = "Open File";
            openFileDialog.Filter = "Flux Query files (*.flux)|*.flux|Text files (*.txt)|*.txt|All Files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                DocumentTab newtab = DocumentTab.FromFile(openFileDialog.FileName);
                tabs.Items.Add(newtab);
                tabs.SelectedItem = newtab;
            }
        }

        private void DuplicateFile(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentTab != null)
            {
                DocumentTab newtab = DocumentTab.New(MakeNewFileName(CurrentTab.Header.ToString()));
                tabs.Items.Add(newtab);
                CurrentTab.Panel.CopyTo(newtab.Panel);
                tabs.SelectedItem = newtab;
            }
        }

        private void ShowSettings(object sender, ExecutedRoutedEventArgs e)
        {
            ShowSettings();
        }

        public void ShowSettings(Type page = null)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Owner = Window.GetWindow(this);
            if (page != null)
            {
                sw.SelectPage(page);
            }
            sw.ShowDialog();
            SettingsChanged();
        }

        private string MakeNewFileName(string filename)
        {
            // Chop off the file extension, if there is any
            int extdot = filename.LastIndexOf('.');
            string ext = string.Empty;
            if (extdot > -1)
            {
                ext = filename.Substring(extdot);
                filename = filename.Substring(0, extdot);
            }

            // Check if there is a number at the end of the filename
            int number = 0;
            Regex integerfinder = new Regex("(\\d+)$");
            Match match = integerfinder.Match(filename);
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out number);
            }

            // Increase the number to one that makes a unique tab filename
            filename = filename.Substring(0, filename.Length - match.Groups[1].Value.Length);
            string newfilename;
            List<DocumentTab> tabitems = GetTabs();
            do
            {
                number++;
                newfilename = filename + number + ext;
            }
            while (tabitems.Any(t => t.Header.ToString() == newfilename));

            return newfilename;
        }

        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            SaveTabToFile(CurrentTab);
        }

        private void SaveFileAs(object sender, ExecutedRoutedEventArgs e)
        {
            SaveTabToFileAs(CurrentTab);
        }

        private void SaveTabToFile(DocumentTab t)
        {
            if (string.IsNullOrEmpty(t.FilePathName))
            {
                SaveTabToFileAs(t);
            }
            else
            {
                try
                {
                    t.SaveToFile(t.FilePathName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Unable to write the file \"{t.FilePathName}\".\n{ex.Message}", "Save file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveTabToFileAs(DocumentTab t)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = t.FilePathName;
            saveFileDialog.Title = "Save File As";
            saveFileDialog.Filter = "Flux Query files (*.flux)|*.flux|Text files (*.txt)|*.txt|All Files (*.*)|*.*";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    t.SaveToFile(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Unable to write the file \"{t.FilePathName}\".\n{ex.Message}", "Save file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<DocumentTab> changedtabs = GetTabs().Where(t => t.HasChanged).ToList();
            if(changedtabs.Count > 0)
            {
                string filelist = changedtabs.Aggregate(string.Empty, (list, t) => list + t.Title + "\n", list => list.Trim());
                MessageBoxResult result = MessageBox.Show(this, $"The following files have unsaved changes:\n\n{filelist}\n\nDo you want to save changes to the files?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
                switch(result)
                {
                    case MessageBoxResult.Yes:
                        // Save files, then continue to exit
                        changedtabs.ForEach(t => SaveTabToFile(t));
                        break;

                    case MessageBoxResult.No:
                        // Continue with exit
                        break;

                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;

                    default:
                        throw new NotImplementedException();
                }
            }

            Settings.Default.Save();
        }
    }
}
