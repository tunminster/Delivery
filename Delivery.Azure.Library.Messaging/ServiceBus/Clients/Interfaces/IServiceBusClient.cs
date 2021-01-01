using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections.Interfaces;
using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Messaging.ServiceBus.Clients.Interfaces
{
    public interface IServiceBusClient
    {
        /// <summary>
        ///     Encapsulates information about the service bus connection
        /// </summary>
        /// <remarks>This is managed by a registered <see cref="IServiceBusSenderConnectionManager" /></remarks>
        IConnectionMetadata ConnectionMetadata { get; }

        /// <summary>
        ///     Sends the message to the endpoint configured in the <see cref="IConnectionMetadata" />
        /// </summary>
        /// <param name="message">The message to send</param>
        Task<string> SendAsync(Message message);
    }
}