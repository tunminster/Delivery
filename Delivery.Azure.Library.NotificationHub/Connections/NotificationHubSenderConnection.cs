using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Microsoft.Azure.NotificationHubs;

namespace Delivery.Azure.Library.NotificationHub.Connections
{
    public class NotificationHubSenderConnection : Connection.Managers.Connection
    {
        /// <summary>
        ///     Current partition that is being used
        /// </summary>
        /// <param name="connectionMetadata">Metadata describing the connection</param>
        public NotificationHubSenderConnection(IConnectionMetadata connectionMetadata, NotificationHubClient hub) : base(connectionMetadata)
        {
            Hub = hub;
        }
        
        public NotificationHubClient Hub { get; }
        
    }
}