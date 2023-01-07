using System.Runtime.Serialization;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts;
using Delivery.Azure.Library.Core.Extensions;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Contracts.Contracts
{
    public record ExecutingRequestContext : AnonymousExecutingRequestContext
    {
        /// <summary>
        ///     A shard is a way to conceptually split databases or other resources into physically separated shards which share a
        ///     common schema
        /// </summary>
        public string? ShardKey { get; set; }
        
        /// <summary>
        ///     Allows to pass the authenticated user details from the api to downstream receivers.
        /// </summary>
        public AuthenticatedUserContract? AuthenticatedUser { get; set; }
    }
}