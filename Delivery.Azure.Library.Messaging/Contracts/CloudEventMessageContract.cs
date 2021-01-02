using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Messaging.Contracts
{
    [DataContract]
    public class CloudEventMessageContract
    {
        [DataMember]
        public string Specversion { get; set; }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public string CorrelationId { get; set; }

        [DataMember]
        public string Shard { get; set; }

        [DataMember]
        public int Ring { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Time { get; set; }

        [DataMember]
        public string Datacontenttype { get; set; }

        [DataMember]
        public object Data { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name} - {nameof(MessageId)}: {MessageId.Format()}, {nameof(CorrelationId)}: {CorrelationId.Format()}, {nameof(Shard)}: {Shard.Format()}";
        }
    }
}