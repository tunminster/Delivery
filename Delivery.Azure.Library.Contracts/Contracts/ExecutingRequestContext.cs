using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;
using Delivery.Azure.Library.Core.Extensions;

namespace Delivery.Azure.Library.Contracts.Contracts
{
    [DataContract]
    public class ExecutingRequestContext : IVersionedContract
    {
        /// <summary>
        /// The targeted ring deployment environment
        /// </summary>
        [DataMember]
        public int? Ring { get; set; }

        /// <summary>
        ///     A shard is a way to conceptually split databases or other resources into physically separated shards which share a
        ///     common schema
        /// </summary>
        [DataMember]
        public string? ShardKey { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public string? CorrelationId { get; set; }

        /// <summary>
        ///     Allows to pass the authenticated user details from the api to downstream receivers.
        /// </summary>
        [DataMember]
        public AuthenticatedUserContract? AuthenticatedUser { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}:" +
                   $"{nameof(Ring)}: {Ring.Format()}, " +
                   $"{nameof(ShardKey)}: {ShardKey.Format()}, " +
                   $"{nameof(CorrelationId)}: {CorrelationId.Format()}, " +
                   $"{nameof(AuthenticatedUser)}: {AuthenticatedUser.Format()}";
        }

        public int Version { get; } = 1;
    }
}