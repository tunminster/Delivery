using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreUpdate;
using Delivery.Store.Domain.Handlers.MessageHandlers.StoreUpdate;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreUpdates
{
    public class StoreUpdateService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StoreUpdateService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreUpdateServiceResult> ExecuteStoreUpdateWorkflowAsync(
            StoreUpdateServiceRequest request)
        {
            var storeCreationMessageContract = new StoreUpdateMessage
            {
                PayloadIn = request.StoreUpdateContract,
                PayloadOut = request.StoreUpdateStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new StoreUpdateMessagePublisher(serviceProvider).PublishAsync(storeCreationMessageContract);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(ExecuteStoreUpdateWorkflowAsync)} published store update message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

            return new StoreUpdateServiceResult(request.StoreUpdateStatusContract);
        }
    }
}