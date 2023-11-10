using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluxtreme
{
    public class FluxRecordView
    {
        private FluxRecord record;

        public object this[int value] => record.GetValueByIndex(value);

        public FluxRecordView(FluxRecord record)
        {
            this.record = record;
        }
    }
}
