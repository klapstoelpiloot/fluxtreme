using InfluxDB.Client.Core.Flux.Domain;

namespace CodeImp.Fluxtreme.Viewers
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
