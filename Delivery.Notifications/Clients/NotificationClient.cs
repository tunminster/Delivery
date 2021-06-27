using System;
using Microsoft.Azure.NotificationHubs;

namespace Delivery.Notifications.Clients
{
    public class NotificationClient
    {
        private readonly IServiceProvider serviceProvider;
        
        public static NotificationClient Instance = new NotificationClient();
        public NotificationHubClient Hub { get; set; }
        
        private NotificationClient() {
            Hub = NotificationHubClient.CreateClientFromConnectionString("<your hub's DefaultFullSharedAccessSignature>",
                "<hub name>");
        }
    }
}