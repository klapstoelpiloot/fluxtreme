using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Configuration
{
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class DatasourceSettingsCollection : ObservableCollection<DatasourceSettings>
    {
    }
}
