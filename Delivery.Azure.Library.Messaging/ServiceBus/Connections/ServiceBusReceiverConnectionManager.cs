using System;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections.Interfaces;
using Microsoft.Azure.ServiceBus.Core;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Connections
{
    public class ServiceBusReceiverConnectionManager : ServiceBusConnectionManager<ServiceBusReceiverConnection>, IServiceBusReceiverConnectionManager
    {
        public ServiceBusReceiverConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ServiceBusReceiverConnection CreateConnection(IConnectionMetadata connectionMetadata, string connectionString, string queueOrTopicName)
        {
            var messageSender = new MessageReceiver(connectionString, queueOrTopicName);

            var connectionInfo = new ServiceBusReceiverConnection(connectionMetadata, messageSender);

            return connectionInfo;
        }
    }
}