using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverStripeOnBoardingLink;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverStripeOnBoardingLink
{
    public record DriverStripeOnBoardingLinkCommand(
        DriverOnBoardingLinkCreationContract DriverOnBoardingLinkCreationContract);
    public class DriverStripeOnBoardingLinkCommandHandler : ICommandHandler<DriverStripeOnBoardingLinkCommand, DriverOnBoardingLinkStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverStripeOnBoardingLinkCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverOnBoardingLinkStatusContract> HandleAsync(DriverStripeOnBoardingLinkCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;

            var options = new AccountCreateOptions
            {
                Country = "US",
                Type = "express",
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true
                    },
                },
                BusinessType = "individual",
                Email = command.DriverOnBoardingLinkCreationContract.EmailAddress,
                DefaultCurrency = "USD"
            };

            var service = new AccountService();
            var accountContract = await service.CreateAsync(options);

            var accountLinkOptions = new AccountLinkCreateOptions
            {
                Account = accountContract.Id,
                RefreshUrl = serviceProvider.GetRequiredService<IConfigurationProvider>()
                    .GetSettingOrDefault("OnBoarding-Refresh-Url", "https://www.ragibull.com/delivery-partner"),
                ReturnUrl = serviceProvider.GetRequiredService<IConfigurationProvider>()
                    .GetSettingOrDefault("OnBoarding-Refresh-Url",
                        "https://www.ragibull.com/delivery-partner-thank-you"),
                Type = "account_onboarding"
            };
            var accountLinkService = new AccountLinkService();
            var accountLinkContract = await accountLinkService.CreateAsync(accountLinkOptions);

            var driverOnBoardingLinkStatusContract = new DriverOnBoardingLinkStatusContract
            {
                Status = true,
                OnBoardingLink = accountLinkContract.Url
            };

            return driverOnBoardingLinkStatusContract;
        }
    }
}