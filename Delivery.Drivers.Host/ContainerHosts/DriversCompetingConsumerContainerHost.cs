using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Messaging.Extensions;
using Delivery.Azure.Library.Messaging.Serialization;
using Delivery.Azure.Library.Messaging.ServiceBus;
using Delivery.Azure.Library.Microservices.Hosting.Hosts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics;
using Delivery.Domain.Contracts.Enums;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverIndex;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverPayments;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverProfile;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverActive;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverIndex;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverProfile;
using Delivery.Shop.Domain.Contracts.V1.MessageContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Handlers.MessageHandlers.ShopOrderManagement;
using Delivery.StripePayment.Domain.Contracts.V1.MessageContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.SplitPayments;
using Delivery.StripePayment.Domain.Handlers.MessageHandlers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Drivers.Host.ContainerHosts
{
    public class DriversCompetingConsumerContainerHost : CompetingConsumerContainerHost<MessageProcessingStates>
    {
        public DriversCompetingConsumerContainerHost(IHostBuilder hostBuilder) : base(hostBuilder)
        {
        }

        public override string QueueOrTopicName => ServiceProvider.GetRequiredService<IConfigurationProvider>()
            .GetSetting("Topic_Name").ToLowerInvariant();

        protected override async Task ProcessMessageAsync(Message message, string correlationId,
            ApplicationInsightsStopwatch stopwatch,
            Dictionary<string, string> telemetryContextProperties)
        {
            var processingState = message.GetMessageProcessingState<MessageProcessingStates>();
            var ring = message.GetRing();

            if (Ring != ring)
            {
                throw new InvalidOperationException(
                    $"The ring specified in the message {ring} does not match the ring which this {GetType().Name} can handle (ring: {Ring}). Properties: {telemetryContextProperties.Format()}");
            }

            if (!message.UserProperties.TryGetValue(UserProperties.MessageType, out var messageType))
            {
                throw new InvalidOperationException($"Message needs to have a {UserProperties.MessageType} header");
            }

            switch (messageType)
            {
                case nameof(DriverCreationMessageContract):
                    var driverCreationMessageContract = message.Deserialize<DriverCreationMessageContract>();
                    var driverCreationMessageHandler = new DriverCreationMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverCreationMessageContract.RequestContext));
                    await driverCreationMessageHandler.HandleMessageAsync(driverCreationMessageContract,
                        processingState);
                    break;
                
                case nameof(DriverAssignmentMessageContract):
                    var driverAssignmentMessageContract = message.Deserialize<DriverAssignmentMessageContract>();
                    var driverAssignmentMessageHandler = new DriverAssignmentMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverAssignmentMessageContract.RequestContext));
                    await driverAssignmentMessageHandler.HandleMessageAsync(driverAssignmentMessageContract,
                        processingState);
                    break;
                
                case nameof(DriverActiveMessageContract):
                    var driverActiveMessageContract = message.Deserialize<DriverActiveMessageContract>();
                    var driverActiveMessageHandler = new DriverActiveMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverActiveMessageContract.RequestContext));
                    await driverActiveMessageHandler.HandleMessageAsync(driverActiveMessageContract, processingState);
                    break;
                
                case nameof(DriverRequestMessageContract):
                    var driverOrderRejectionMessageContract =
                        message.Deserialize<DriverRequestMessageContract>();
                    var driverShopOrderDriverRequestMessageHandler = new ShopOrderDriverRequestMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverOrderRejectionMessageContract.RequestContext));
                    var shopOrderDriverRequestMessageContract = new ShopOrderDriverRequestMessageContract
                    {
                        PayloadIn = new ShopOrderDriverRequestContract
                        {
                            OrderId = driverOrderRejectionMessageContract.PayloadIn!.OrderId
                        },
                        PayloadOut = driverOrderRejectionMessageContract.PayloadOut,
                        RequestContext = driverOrderRejectionMessageContract.RequestContext
                    };
                    await driverShopOrderDriverRequestMessageHandler.HandleMessageAsync(shopOrderDriverRequestMessageContract,
                        processingState);
                    break;
                
                case nameof(DriverReAssignmentMessage):
                    var driverReAssignmentMessage = message.Deserialize<DriverReAssignmentMessage>();
                    await new DriverReAssignmentMessageHandler(ServiceProvider,
                            new ExecutingRequestContextAdapter(driverReAssignmentMessage.RequestContext))
                        .HandleMessageAsync(driverReAssignmentMessage, processingState);
                    break;
                
                case nameof(DriverOrderActionMessageContract):
                    var driverOrderActionMessageContract = message.Deserialize<DriverOrderActionMessageContract>();
                    var driverOrderActionMessageHandler = new DriverOrderActionMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverOrderActionMessageContract.RequestContext));
                    await driverOrderActionMessageHandler.HandleMessageAsync(driverOrderActionMessageContract,
                        processingState);
                    break;
                case nameof(DriverServiceAreaUpdateMessageContract):
                    var driverServiceAreaMessageContract =
                        message.Deserialize<DriverServiceAreaUpdateMessageContract>();
                    var driverServiceAreaUpdateMessageHandler = new DriverServiceAreaUpdateMessageHandler(
                        ServiceProvider,
                        new ExecutingRequestContextAdapter(driverServiceAreaMessageContract.RequestContext));
                    await driverServiceAreaUpdateMessageHandler.HandleMessageAsync(driverServiceAreaMessageContract,
                        processingState);
                    break;
                case nameof(DriverOrderCompleteMessageContract):
                    var driverOrderCompleteMessageContract = message.Deserialize<DriverOrderCompleteMessageContract>();
                    var shopOrderCompleteMessageContract = new ShopOrderCompleteMessageContract
                    {
                        PayloadIn = driverOrderCompleteMessageContract.PayloadIn,
                        PayloadOut = driverOrderCompleteMessageContract.PayloadOut,
                        RequestContext = driverOrderCompleteMessageContract.RequestContext
                    };
                    var shopOrderCompleteMessageHandler = new ShopOrderCompleteMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(shopOrderCompleteMessageContract.RequestContext));
                    await shopOrderCompleteMessageHandler.HandleMessageAsync(shopOrderCompleteMessageContract,
                        processingState);
                    break;
                case nameof(DriverOrderInProgressMessageContract):
                    var driverOrderInProgressMessageContract =
                        message.Deserialize<DriverOrderInProgressMessageContract>();
                    var shopOrderDeliverOnWayMessageContract = new ShopOrderDeliverOnWayMessageContract
                    {
                        PayloadIn = driverOrderInProgressMessageContract.PayloadIn,
                        PayloadOut = driverOrderInProgressMessageContract.PayloadOut,
                        RequestContext = driverOrderInProgressMessageContract.RequestContext
                    };
                    var shopOrderDeliverOnWayMessageHandler = new ShopOrderDeliverOnWayMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(shopOrderDeliverOnWayMessageContract.RequestContext));
                    await shopOrderDeliverOnWayMessageHandler.HandleMessageAsync(shopOrderDeliverOnWayMessageContract,
                        processingState);
                    break;
                case nameof(DriverOrderIndexMessageContract):
                    var driverOrderIndexMessageContract = message.Deserialize<DriverOrderIndexMessageContract>();
                    await new DriverOrderIndexMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(driverOrderIndexMessageContract.RequestContext))
                        .HandleMessageAsync(driverOrderIndexMessageContract, processingState);
                    break;
                
                case nameof(DriverIndexMessageContract):
                    var driverIndexMessageContract = message.Deserialize<DriverIndexMessageContract>();
                    await new DriverIndexMessageHandler(ServiceProvider,
                            new ExecutingRequestContextAdapter(driverIndexMessageContract.RequestContext))
                        .HandleMessageAsync(driverIndexMessageContract, processingState);
                    break;
                
                case nameof(DriverPaymentCreationMessageContract):
                    var driverPaymentCreationMessage = message.Deserialize<DriverPaymentCreationMessageContract>();
                    var splitPaymentMessage = new SplitPaymentCreationMessageContract
                    {
                        PayloadIn = new SplitPaymentCreationContract
                        {
                            OrderId = driverPaymentCreationMessage.PayloadIn!.OrderId,
                            DriverConnectedAccountId = driverPaymentCreationMessage.PayloadIn!.DriverConnectAccountId
                        },
                        PayloadOut = driverPaymentCreationMessage.PayloadOut,
                        RequestContext = driverPaymentCreationMessage.RequestContext
                    };
                    var splitPMessageHandler = new SplitPaymentsMessageHandler(ServiceProvider,
                        new ExecutingRequestContextAdapter(splitPaymentMessage.RequestContext));
                    await splitPMessageHandler.HandleMessageAsync(splitPaymentMessage, processingState);
                    break;
                default:
                    throw new NotImplementedException($"Message type {messageType} is not implemented.");
            }
        }
    }
}