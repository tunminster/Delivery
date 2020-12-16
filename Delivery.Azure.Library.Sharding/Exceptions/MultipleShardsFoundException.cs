using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Core.Guards;
using Delivery.Azure.Library.Sharding.Interfaces;

namespace Delivery.Azure.Library.Sharding.Exceptions
{
    [Serializable]
    public class MultipleShardsFoundException : Exception
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="shards">Shards that were found</param>
        public MultipleShardsFoundException(IEnumerable<IShard> shards) : base($"Multiple X-Shard headers were found. ({shards.Format()})")
        {
            Guard.AgainstEmptyCollection(shards, nameof(shards));

            Shards = shards.ToList();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="shards">Shards that were found</param>
        /// <param name="innerException">Related exception that caused this to occur</param>
        public MultipleShardsFoundException(IEnumerable<IShard> shards, Exception innerException) : base($"Multiple X-Shard headers were found. ({shards.Format()})", innerException)
        {
            Guard.AgainstEmptyCollection(shards, nameof(shards));

            Shards = shards.ToList();
        }

        protected MultipleShardsFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        ///     Shards that were found
        /// </summary>
        public List<IShard> Shards { get; } = new List<IShard>();
    }
}