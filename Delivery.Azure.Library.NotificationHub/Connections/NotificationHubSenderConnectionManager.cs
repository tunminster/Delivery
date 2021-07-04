using System;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.NotificationHub.Connections.Interfaces;
using Microsoft.Azure.NotificationHubs;

namespace Delivery.Azure.Library.NotificationHub.Connections
{
    public class NotificationHubSenderConnectionManager : NotificationHubConnectionManager<NotificationHubSenderConnection>, INotificationHubSenderConnectionManager
    {
        public NotificationHubSenderConnectionManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override NotificationHubSenderConnection CreateConnection(IConnectionMetadata connectionMetadata,
            string defaultFullSharedAccessSignature, string hubName)
        {
            var hub = NotificationHubClient.CreateClientFromConnectionString(defaultFullSharedAccessSignature,
                hubName);
            
            var connectionInfo = new NotificationHubSenderConnection(connectionMetadata, hub);

            return connectionInfo;
        }
    }
}