using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation
{
    public class AccountLinkCreationCommandHandler :  ICommandHandler<AccountLinkCreationCommand, StripeAccountLinkCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;

        public AccountLinkCreationCommandHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public async Task<StripeAccountLinkCreationStatusContract> Handle(AccountLinkCreationCommand command)
        {
            var stripeApiKey = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("Stripe-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            var options = new AccountLinkCreateOptions
            {
                Account = command.StripeAccountLinkCreationContract.AccountId,
                RefreshUrl = command.StripeAccountLinkCreationContract.RefreshUrl,
                ReturnUrl = command.StripeAccountLinkCreationContract.ReturnUrl,
                Type = "account_onboarding",
            };
            var service = new AccountLinkService();
            var accountLink = await service.CreateAsync(options);

            var stripeAccountLinkCreateStatusContract = new StripeAccountLinkCreationStatusContract
            {
                AccountId = command.StripeAccountLinkCreationContract.AccountId,
                AccountLink = accountLink.Url,
                ExpiresAt = accountLink.ExpiresAt
            };

            return stripeAccountLinkCreateStatusContract;
            
        }
    }
}