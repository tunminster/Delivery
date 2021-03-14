using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Stripe;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.LoginLinkCreation
{
    public class LoginLinkCreationCommandHandler : ICommandHandler<LoginLinkCreationCommand, StripeLoginLinkCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public LoginLinkCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeLoginLinkCreationStatusContract> Handle(LoginLinkCreationCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var service = new LoginLinkService();

            var loginLink = await Policy.HandleResult<LoginLink>(x => x.StripeResponse.StatusCode != HttpStatusCode.OK)
                .Or<Exception>()
                .WaitAndRetryAsync(retryCount: 3, retryAttempt => TimeSpan.FromSeconds(value: 2))
                .ExecuteAsync(async () =>
                {
                    var dependencyData = new DependencyData(nameof(StripeLoginLinkCreationContract),
                        command.StripeLoginLinkCreationContract);
                    var dependencyTarget = service.BasePath;
                    var correlationId = executingRequestContextAdapter.GetCorrelationId();

                    var customProperties = executingRequestContextAdapter.GetTelemetryProperties();
                    customProperties.Add("Request", command.StripeLoginLinkCreationContract.ConvertToJson());

                    var loginLinkResult = await new DependencyMeasurement(serviceProvider)
                        .ForDependency("Stripe", MeasuredDependencyType.WebService, dependencyData.ConvertToJson(),
                            dependencyTarget)
                        .WithCorrelationId(correlationId)
                        .WithContextualInformation(customProperties)
                        .TrackAsync(async () =>
                        {
                            var loginLinkResponse =
                                await service.CreateAsync(command.StripeLoginLinkCreationContract.AccountId);
                            return loginLinkResponse;
                        });
                    return loginLinkResult;
                });
            
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{nameof(LoginLinkService)} requested login link url.");

            var stripeLoginLinkCreationStatusContract = new StripeLoginLinkCreationStatusContract
            {
                AccountId = command.StripeLoginLinkCreationContract.AccountId,
                LoginUrl = loginLink.Url,
                Created = loginLink.Created
            };

            return stripeLoginLinkCreationStatusContract;
        }
    }
}