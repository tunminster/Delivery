using System;
using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Core.Guards;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;

namespace Delivery.Azure.Library.Microservices.Hosting.MessageHandlers
{
    public class MultiShardedTelemeterizedMessageHandler : TelemeterizedMessageHandler
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="shards">Current shards that are being used</param>
        /// <param name="correlationId">Id used to correlated all telemetry</param>
        /// <param name="executingRequestContextAdapter"></param>
        public MultiShardedTelemeterizedMessageHandler(IServiceProvider serviceProvider, IEnumerable<IShard> shards, string correlationId, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
            Guard.AgainstEmptyCollection(shards, nameof(shards));

            Shards.AddRange(shards);
            CorrelationId = correlationId;

            if (shards.Count() == 1)
            {
                AddOrUpdateTelemetryContextProperty($"{CustomProperties.Shard}", Shards[index: 0].Key);
            }

            for (var i = 0; i < Shards.Count; i++)
            {
                AddOrUpdateTelemetryContextProperty($"{CustomProperties.Shard}-{i}", Shards[i].Key);
            }
        }

        public List<IShard> Shards { get; } = new();

        public string CorrelationId { get; }
    }
}