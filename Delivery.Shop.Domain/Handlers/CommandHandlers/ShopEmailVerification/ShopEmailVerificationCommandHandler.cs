using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.EmailVerifications;
using Delivery.Library.Twilio.Extensions;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification;
using Delivery.Shop.Domain.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopEmailVerification
{
    public record ShopEmailVerificationCommand(ShopEmailVerificationContract ShopEmailVerificationContract);
    
    public class ShopEmailVerificationCommandHandler : ICommandHandler<ShopEmailVerificationCommand, ShopEmailVerificationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopEmailVerificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopEmailVerificationStatusContract> Handle(ShopEmailVerificationCommand command)
        {
            var twilioAccountSid = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Account-Sid");
            var twilioAuthToken = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Auth-Token");
            var twilioEmailVerifyServiceId = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Email-Verify-Service-Id");
            
            var twilioProvider = await TwilioEmailVerificationProvider.CreateAsync(serviceProvider, new TwilioEmailVerifyServiceConfiguration(twilioAccountSid, twilioAuthToken, twilioEmailVerifyServiceId));
            var twilioEmailVerificationStatusContract = await twilioProvider.SendVerificationEmail(command.ShopEmailVerificationContract
                .ConvertToTwilio().WithExecutingContext(executingRequestContextAdapter));
            
            return twilioEmailVerificationStatusContract.ConvertToShopEmailVerificationStatusContract();
        }
    }
}