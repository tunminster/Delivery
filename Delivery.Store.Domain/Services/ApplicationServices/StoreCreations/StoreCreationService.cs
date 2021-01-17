using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreCreations;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreCreation;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreCreations
{
    public class StoreCreationService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreCreationService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<StoreCreationServiceResult> ExecuteStoreCreationWorkflowAsync(
            StoreCreationServiceRequest request)
        {
            var storeCreationMessageContract = new StoreCreationMessageContract
            {
                PayloadIn = request.StoreCreationContract,
                PayloadOut = request.StoreCreationStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new StoreCreationMessagePublisher(serviceProvider).PublishAsync(storeCreationMessageContract);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(ExecuteStoreCreationWorkflowAsync)} published store creation message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

           return new StoreCreationServiceResult(request.StoreCreationStatusContract);
        }
    }
}