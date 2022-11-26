using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.Contracts.Enums;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOnBoardingEmail;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverStripeOnBoardingLink;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverCreation;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverOnBoardingEmail;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverStripeOnBoardingLink;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.MessageHandlers
{
    public class DriverCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public DriverCreationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(DriverCreationMessageContract message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableResponseMessageAdapter<DriverCreationContract, DriverCreationStatusContract>(message);

                var driverCreationContract = messageAdapter.GetPayloadIn();

                var onBoardingLink = string.Empty;

                if (!processingStates.HasFlag(MessageProcessingStates.Persisted))
                {
                    var driverCreationCommand =
                        new DriverCreationCommand(messageAdapter.GetPayloadIn(), messageAdapter.GetPayloadOut());

                    await new DriverCreationCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(driverCreationCommand);
                    
                    processingStates |= MessageProcessingStates.Persisted;
                }

                if (!processingStates.HasFlag(MessageProcessingStates.OnBoardingLinkCreated))
                {
                    var driverStripeOnBoardingLinkCommand = new DriverStripeOnBoardingLinkCommand(
                        new DriverOnBoardingLinkCreationContract
                            {EmailAddress = driverCreationContract.EmailAddress});

                    var driverOnBoardingLinkStatusContract = await new DriverStripeOnBoardingLinkCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(driverStripeOnBoardingLinkCommand);

                    onBoardingLink = driverOnBoardingLinkStatusContract.OnBoardingLink;
                    
                    processingStates |= MessageProcessingStates.OnBoardingLinkCreated;
                }

                // todo: on-boarding notification should not be sent because we are using stripe custom account type.
                // if (processingStates.HasFlag(MessageProcessingStates.OnBoardingLinkCreated) && 
                //     !processingStates.HasFlag(MessageProcessingStates.NotificationSent) && !string.IsNullOrEmpty(onBoardingLink))
                // {
                //     var driverOnBoardingEmailCommand = new DriverOnBoardingEmailCommand(
                //         new DriverOnBoardingEmailCreationContract
                //         {
                //             Email = driverCreationContract.EmailAddress, 
                //             Name = driverCreationContract.FullName,
                //             OnBoardingLink = onBoardingLink
                //         });
                //
                //     await new DriverOnBoardingEmailCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                //         .Handle(driverOnBoardingEmailCommand);
                //
                //     processingStates |= MessageProcessingStates.NotificationSent;
                // }

                if (processingStates.HasFlag(MessageProcessingStates.Persisted) &&
                    !processingStates.HasFlag(MessageProcessingStates.Indexed))
                {
                    var driverIndexCommand =
                        new DriverIndexCommand(messageAdapter.GetPayloadOut().DriverId);

                    await new DriverIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(driverIndexCommand);
                    
                    processingStates |= MessageProcessingStates.Indexed;
                }
                
                // complete
                processingStates |= MessageProcessingStates.Processed;
                
                
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Driver application persisted",
                    value: 1, ExecutingRequestContextAdapter.GetTelemetryProperties());

            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}