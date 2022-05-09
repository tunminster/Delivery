using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation
{
    public class AccountLinkCreationCommandHandler :  ICommandHandler<AccountLinkCreationCommand, StripeAccountLinkCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public AccountLinkCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripeAccountLinkCreationStatusContract> HandleAsync(AccountLinkCreationCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
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