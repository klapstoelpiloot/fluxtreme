using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluxtreme
{
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class DatasourceSettings
    {
        /// <summary>
        /// Name to display for this datasource.
        /// </summary>
        public string Name { get; set; } = "Local database";

        /// <summary>
        /// IP address of the InfluxDB database in the format "address[:port]".
        /// </summary>
        public string Address { get; set; } = "127.0.0.1";

        /// <summary>
        /// Access token to gain access to the database.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Organization ID used to query the database.
        /// </summary>
        public string OrganizationID { get; set; }

        /// <summary>
        /// Default bucket for queries. This is used for the query variable v.defaultBucket.
        /// </summary>
        public string DefaultBucket { get; set; }

        public DatasourceSettings()
        {
        }
    }
}
