using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Messaging.Messages.V1
{
    [DataContract]
    public abstract class AuditableRequestMessage<TPayloadIn> : IVersionedContract
        where TPayloadIn : class, new()
    {
        [DataMember]
        public virtual int Version { get; } = 1;

        [DataMember]
        public TPayloadIn? PayloadIn { get; set; }

        [DataMember]
        public ExecutingRequestContext? RequestContext { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}: {nameof(PayloadIn)}: {PayloadIn.Format()}, {nameof(RequestContext)}: {RequestContext.Format()}";
        }
    }
}