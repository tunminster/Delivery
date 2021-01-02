using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Microsoft.Azure.ServiceBus.Core;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Connections
{
    public class ServiceBusReceiverConnection : Connection.Managers.Connection
    {
        /// <summary>
        ///     Current partition that is being used
        /// </summary>
        /// <param name="connectionMetadata">Metadata describing the connection</param>
        /// <param name="messageReceiver">Connection to the queue or topic to receive messages</param>
        public ServiceBusReceiverConnection(IConnectionMetadata connectionMetadata, MessageReceiver messageReceiver) : base(connectionMetadata)
        {
            MessageReceiver = messageReceiver;
        }

        public MessageReceiver MessageReceiver { get; }

        public override async ValueTask DisposeAsync()
        {
            await MessageReceiver.CloseAsync();
        }
    }
}