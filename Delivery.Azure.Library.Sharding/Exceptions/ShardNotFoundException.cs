using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Sharding.Exceptions
{
    [Serializable]
    public class ShardNotFoundException : Exception
    {
        private const string DefaultMessage = "Shard '{0}' was not found";

        public ShardNotFoundException() : base("No shards were found")
        {
        }

        public ShardNotFoundException(IShard shard) : this(shard, string.Format(DefaultMessage, shard.Key))
        {
        }

        public ShardNotFoundException(IShard shard, Exception innerException) : this(shard, string.Format(DefaultMessage, shard.Key), innerException)
        {
        }

        public ShardNotFoundException(IShard shard, string message) : base(message)
        {
            Shard = shard;
        }

        public ShardNotFoundException(IShard shard, string message, Exception innerException) : base(message, innerException)
        {
            Shard = shard;
        }

        protected ShardNotFoundException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        /// <summary>
        ///     Shard that was not found
        /// </summary>
        public IShard? Shard { get; }
    }
}