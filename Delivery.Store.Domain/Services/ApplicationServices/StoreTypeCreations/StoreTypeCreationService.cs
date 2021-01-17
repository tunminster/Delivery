using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreTypeCreations;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreTypeCreations;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreTypeCreations
{
    public class StoreTypeCreationService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreTypeCreationService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreTypeCreationServiceResult> ExecuteStoreTypeCreationWorkflowAsync(
            StoreTypeCreationServiceRequest request)
        {
            var storeCreationMessageContract = new StoreTypeCreationMessageContract
            {
                PayloadIn = request.StoreTypeCreationContract,
                PayloadOut = request.StoreTypeCreationStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new StoreTypeCreationMessagePublisher(serviceProvider).PublishAsync(storeCreationMessageContract);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(ExecuteStoreTypeCreationWorkflowAsync)} published store type creation message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

            return new StoreTypeCreationServiceResult(request.StoreTypeCreationStatusContract);
        }
    }
}