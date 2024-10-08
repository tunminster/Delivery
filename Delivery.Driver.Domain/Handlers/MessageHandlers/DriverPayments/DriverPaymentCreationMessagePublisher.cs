using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.Messaging.HostedServices.Interfaces;
using Delivery.Azure.Library.Messaging.ServiceBus.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Constants;
using Delivery.Domain.FrameWork.Messages;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverPayments;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers.DriverPayments
{
    public class DriverPaymentCreationMessagePublisher : IMessagePublisherAsync<DriverPaymentCreationMessageContract>
    {
        private readonly IServiceProvider serviceProvider;

        public DriverPaymentCreationMessagePublisher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public async Task PublishAsync(DriverPaymentCreationMessageContract message)
        {
            var executingContextAdapter = new ExecutingRequestContextAdapter(message.RequestContext);
            var cloudEventMessage = message
                .CreateCloudEventMessage(serviceProvider, executingContextAdapter)
                .WithExecutingContext(executingContextAdapter);
            
            await serviceProvider.GetRequiredHostedService<IQueueServiceBusWorkBackgroundService>()
                .EnqueueBackgroundWorkAsync(Delivery.Domain.Constants.Constants.DriverServiceBusEntityName,
                    Delivery.Domain.Constants.Constants.DriverServiceBusConnectionStringName, cloudEventMessage);
        }
    }
}