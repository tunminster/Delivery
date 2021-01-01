using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Microsoft.Azure.ServiceBus.Core;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Connections
{
    public class ServiceBusSenderConnection : Connection.Managers.Connection
    {
        /// <summary>
        ///     Current partition that is being used
        /// </summary>
        /// <param name="connectionMetadata">Metadata describing the connection</param>
        /// <param name="messageSender">Connection to the queue or topic to send messages</param>
        public ServiceBusSenderConnection(IConnectionMetadata connectionMetadata, MessageSender messageSender) : base(connectionMetadata)
        {
            MessageSender = messageSender;
        }

        public MessageSender MessageSender { get; }

        public override async ValueTask DisposeAsync()
        {
            await MessageSender.CloseAsync();
        }
    }
}