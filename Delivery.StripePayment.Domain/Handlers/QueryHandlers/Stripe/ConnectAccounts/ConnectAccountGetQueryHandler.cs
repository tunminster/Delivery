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

namespace Delivery.StripePayment.Domain.Handlers.QueryHandlers.Stripe.ConnectAccounts
{
    public class ConnectAccountGetQueryHandler : IQueryHandler<ConnectAccountGetQuery, StripeList<Account>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ConnectAccountGetQueryHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeList<Account>> Handle(ConnectAccountGetQuery query)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var service = new AccountService();
            
            var dependencyData = new DependencyData(nameof(ConnectAccountGetQuery),
                query);
            var dependencyTarget = service.BasePath;
            var correlationId = executingRequestContextAdapter.GetCorrelationId();
            var customProperties = executingRequestContextAdapter.GetTelemetryProperties();
            customProperties.Add("Request", query.ConvertToJson());
            
            var options = new AccountListOptions
            {
                Limit = query.Limit,
                StartingAfter = query.StartingAfter,
                EndingBefore = query.EndingBefore
            };
            
            var accounts = await new DependencyMeasurement(serviceProvider)
                .ForDependency("Stripe", MeasuredDependencyType.WebService, dependencyData.ConvertToJson(),
                    dependencyTarget)
                .WithCorrelationId(correlationId)
                .WithContextualInformation(customProperties)
                .TrackAsync(async () =>
                {
                    StripeList<Account> results = await service.ListAsync(
                        options
                    );
                    return results;
                });

            return accounts;
        }
    }
}