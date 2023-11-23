using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme
{
    public interface IMainWindow
    {
        void SettingsChanged();
        void ShowSettings(Type page = null);
    }
}
