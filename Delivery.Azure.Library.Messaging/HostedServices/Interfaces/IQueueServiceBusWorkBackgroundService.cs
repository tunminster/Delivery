using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces;
using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Messaging.HostedServices.Interfaces
{
    public interface IQueueServiceBusWorkBackgroundService : IQueueWorkBackgroundService
    {
        /// <summary>
        ///     Adds work to be completed asynchronously to the service bus
        /// </summary>
        Task EnqueueBackgroundWorkAsync(string entityName, string connectionStringName, Message cloudEventMessage);
    }
}