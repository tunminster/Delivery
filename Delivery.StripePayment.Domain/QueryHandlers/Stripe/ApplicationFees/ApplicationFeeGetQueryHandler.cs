using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Domain.QueryHandlers;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.QueryHandlers.Stripe.ApplicationFees
{
    public class ApplicationFeeGetQueryHandler : IQueryHandler<ApplicationFeeGetQuery, StripeList<ApplicationFee>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ApplicationFeeGetQueryHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeList<ApplicationFee>> Handle(ApplicationFeeGetQuery query)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var service = new ApplicationFeeService();
            
            var dependencyData = new DependencyData(nameof(ApplicationFeeGetQuery),
                query);
            var dependencyTarget = service.BasePath;
            var correlationId = executingRequestContextAdapter.GetCorrelationId();
            var customProperties = executingRequestContextAdapter.GetTelemetryProperties();
            customProperties.Add("Request", query.ConvertToJson());
            
            var options = new ApplicationFeeListOptions
            {
                Limit = 3
            };
            
            var applicationFees = await new DependencyMeasurement(serviceProvider)
                .ForDependency("Stripe", MeasuredDependencyType.WebService, dependencyData.ConvertToJson(),
                    dependencyTarget)
                .WithCorrelationId(correlationId)
                .WithContextualInformation(customProperties)
                .TrackAsync(async () =>
                {
                    StripeList<ApplicationFee> results = await service.ListAsync(
                        options
                    );
                    return results;
                });

            return applicationFees;
        }
    }
}