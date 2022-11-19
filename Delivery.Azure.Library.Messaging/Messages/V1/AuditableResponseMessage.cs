using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Messaging.Messages.V1
{
    /// <summary>
    ///   Auditable message
    /// </summary>
    /// <typeparam name="TPayloadIn"></typeparam>
    /// <typeparam name="TPayloadOut"></typeparam>
    [DataContract]
    public abstract class AuditableResponseMessage<TPayloadIn, TPayloadOut> : IVersionedContract
        where TPayloadIn : class, new()
        where TPayloadOut : class, new()
    {
        /// <summary>
        ///  Version number
        /// </summary>
        [DataMember]
        public virtual int Version { get; } = 1;

        /// <summary>
        ///  Payload
        /// </summary>
        [DataMember]
        public TPayloadIn? PayloadIn { get; set; }

        /// <summary>
        /// Payload out
        /// </summary>
        [DataMember]
        public TPayloadOut? PayloadOut { get; set; }

        /// <summary>
        ///  Request context
        /// </summary>
        [DataMember]
        public ExecutingRequestContext? RequestContext { get; set; }

        /// <summary>
        ///  To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name}: {nameof(PayloadIn)}: {PayloadIn.Format()}, {nameof(PayloadOut)}: {PayloadOut.Format()}, {nameof(RequestContext)}: {RequestContext.Format()}";
        }
    }
}