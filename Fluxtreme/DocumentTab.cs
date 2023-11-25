using System.IO;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CodeImp.Fluxtreme
{
    public class DocumentTab : TabItem
    {
        private string title;
        private bool haschanged;

        public DocumentPanel Panel { get; private set; }
        public string FilePathName { get; private set; }
        public string Title { get => title; private set { title = value; UpdateTabHeader(); } }
        public bool HasChanged { get => haschanged; private set { haschanged = value; UpdateTabHeader(); } }

        public DocumentTab()
        {
            Panel = new DocumentPanel();
            Panel.Setup();
            Panel.QueryChanged += Panel_QueryChanged;
            Content = Panel;
        }

        public static DocumentTab New(string fileName)
        {
            DocumentTab newtab = new DocumentTab();
            newtab.Title = fileName;
            return newtab;
        }

        public static DocumentTab FromFile(string fileName)
        {
            string contents = File.ReadAllText(fileName);
            DocumentTab newtab = new DocumentTab();
            newtab.FilePathName = fileName;
            newtab.Title = Path.GetFileName(fileName);
            newtab.Panel.Query = contents;
            newtab.HasChanged = false;
            return newtab;
        }

        public void SaveToFile(string fileName)
        {
            FilePathName = fileName;
            File.WriteAllText(fileName, Panel.Query);
            Title = Path.GetFileName(fileName);
            HasChanged = false;
        }

        private void Panel_QueryChanged(object sender, System.EventArgs e)
        {
            HasChanged = true;
        }

        private void UpdateTabHeader()
        {
            string changedPostfix = HasChanged ? "*" : string.Empty;
            Header = Title + changedPostfix;
        }
    }
}
