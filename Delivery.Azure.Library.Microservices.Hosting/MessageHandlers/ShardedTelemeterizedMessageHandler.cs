using System;
using Delivery.Azure.Library.Sharding.Adapters;

namespace Delivery.Azure.Library.Microservices.Hosting.MessageHandlers
{
    public class ShardedTelemeterizedMessageHandler : TelemeterizedMessageHandler
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="executingRequestContextAdapter">Context properties used to provide more information in the telemetry</param>
        public ShardedTelemeterizedMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }
    }
}