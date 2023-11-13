using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeImp.Fluxtreme.Configuration
{
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class DatasourceSettings : INotifyPropertyChanged
    {
        private string name = "Local database";
        private string address = "127.0.0.1";
        private string accesstoken;
        private string organizationid;
        private string defaultbucket;

        /// <summary>
        /// Name to display for this datasource.
        /// </summary>
        public string Name { get => name; set { name = value; RaisePropertyChanged(nameof(Name)); } }

        /// <summary>
        /// IP address of the InfluxDB database in the format "address[:port]".
        /// </summary>
        public string Address { get => address; set { address = value; RaisePropertyChanged(nameof(Address)); } }

        /// <summary>
        /// Access token to gain access to the database.
        /// </summary>
        public string AccessToken { get => accesstoken; set { accesstoken = value; RaisePropertyChanged(nameof(AccessToken)); } }

        /// <summary>
        /// Organization ID used to query the database.
        /// </summary>
        public string OrganizationID { get => organizationid; set { organizationid = value; RaisePropertyChanged(nameof(OrganizationID)); } }

        /// <summary>
        /// Default bucket for queries. This is used for the query variable v.defaultBucket.
        /// </summary>
        public string DefaultBucket { get => defaultbucket; set { defaultbucket = value; RaisePropertyChanged(nameof(DefaultBucket)); } }

        /// <summary>
        /// Name to display for this datasource in the settings page.
        /// </summary>
        public string LongDescription
        {
            get
            {
                string accessmethod = string.Empty;
                if (!string.IsNullOrWhiteSpace(accesstoken))
                {
                    accessmethod = " with Access Token";
                }
                return name + " at " + address + accessmethod;
            }
        }

        public DatasourceSettings()
        {
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LongDescription)));
        }
    }
}
