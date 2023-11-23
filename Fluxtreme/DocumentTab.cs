using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CodeImp.Fluxtreme
{
    public class DocumentTab : TabItem
    {
        public DocumentPanel Panel { get; private set; }

        public DocumentTab()
        {
            Panel = new DocumentPanel();
            Panel.Setup();
            Content = Panel;
        }
    }
}
