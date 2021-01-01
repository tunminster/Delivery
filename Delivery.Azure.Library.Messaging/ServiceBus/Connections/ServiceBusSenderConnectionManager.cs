using System;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections.Interfaces;
using Microsoft.Azure.ServiceBus.Core;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Connections
{
    public class ServiceBusSenderConnectionManager : ServiceBusConnectionManager<ServiceBusSenderConnection>, IServiceBusSenderConnectionManager
    {
        public ServiceBusSenderConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ServiceBusSenderConnection CreateConnection(IConnectionMetadata connectionMetadata, string connectionString, string queueOrTopicName)
        {
            var messageSender = new MessageSender(connectionString, queueOrTopicName);

            var connectionInfo = new ServiceBusSenderConnection(connectionMetadata, messageSender);

            return connectionInfo;
        }
    }
}