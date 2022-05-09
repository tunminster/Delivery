using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation;
using Delivery.StripePayment.Domain.CommandHandlers.AccountCreation.Stripe.AccountLinkCreation;
using Delivery.StripePayment.Domain.Contracts.Enums;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Handlers.CommandHandlers.AccountCreation;
using Delivery.StripePayment.Domain.Handlers.QueryHandlers.Stripe.AccountLinks;

namespace Delivery.StripePayment.Domain.Services.ApplicationServices.StripeAccounts
{
    public class StripeAccountCreationService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StripeAccountCreationService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<StripeAccountCreationServiceResult> ExecuteStripeAccountCreationWorkflowAsync(StripeAccountCreationServiceRequest request)
        {
            var accountLinkGetQuery = new AccountLinkGetQuery(executingRequestContextAdapter.GetShard().Key,
                request.StripeAccountCreationContract.Email);

            var documentContracts =
                await new AccountLinkGetQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    accountLinkGetQuery);

            if (documentContracts != null && documentContracts.Count > 0)
            {
                var stripeAccountLinkCreationContract = new StripeAccountLinkCreationContract
                {
                    AccountId = documentContracts.FirstOrDefault()?.Data.FirstOrDefault()?.AccountId ?? throw new InvalidOperationException($"{nameof(StripeAccountLinkCreationContract.AccountId)} can't be empty."),
                    RefreshUrl = string.Empty,
                    ReturnUrl = string.Empty
                };

                var accountLinkCreationCommand = new AccountLinkCreationCommand(stripeAccountLinkCreationContract);
                var accountLinkCreationResult =
                    await new AccountLinkCreationCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(accountLinkCreationCommand);

                var stripAccountCreationStatusResult = new StripeAccountCreationStatusContract
                {
                    AccountId = accountLinkCreationResult.AccountId ?? string.Empty,
                    AccountStatus = StripeAccountStatus.AlreadyExisted,
                    OnBoardingAccountUrl = accountLinkCreationResult.AccountLink
                };
                
                return new StripeAccountCreationServiceResult(stripAccountCreationStatusResult);
            }
            
            var accountCreationCommand = new AccountCreationCommand(request.StripeAccountCreationContract);
            var stripAccountCreationStatusContract =
                await new AccountCreationCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                    accountCreationCommand);

            return new StripeAccountCreationServiceResult(stripAccountCreationStatusContract);
        }
    }
}