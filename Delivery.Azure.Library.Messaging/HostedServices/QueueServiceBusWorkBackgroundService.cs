using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Clients;
using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Messaging.HostedServices
{
    public class QueueServiceBusWorkBackgroundService : QueueWorkBackgroundService, IQueueServiceBusWorkBackgroundService
    {
        private readonly bool isQueueWorkInCallingThread;
        
        public QueueServiceBusWorkBackgroundService(IServiceProvider serviceProvider, bool isQueueWorkInCallingThread = false) : base(serviceProvider)
        {
            this.isQueueWorkInCallingThread = isQueueWorkInCallingThread;
        }

        public async Task EnqueueBackgroundWorkAsync(string entityName, string connectionStringName, Message cloudEventMessage)
        {
            var serviceBusClient = await ServiceBusClient.CreateAsync(ServiceProvider, entityName, connectionStringName);
            
            if (isQueueWorkInCallingThread)
            {
                await serviceBusClient.SendAsync(cloudEventMessage);
            }
            else
            {
                EnqueueBackgroundWork(token => serviceBusClient.SendAsync(cloudEventMessage));
            }
        }
    }
}