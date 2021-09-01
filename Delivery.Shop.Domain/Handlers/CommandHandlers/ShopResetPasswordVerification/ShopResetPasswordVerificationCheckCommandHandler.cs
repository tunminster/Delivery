using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.EmailVerifications;
using Delivery.Library.Twilio.Extensions;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopResetPasswordVerification;
using Delivery.Shop.Domain.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopResetPasswordVerification
{
    public record ShopResetPasswordVerificationCheckCommand(ShopResetPasswordCreationContract ShopResetPasswordCreationContract);
    
    public class ShopResetPasswordVerificationCheckCommandHandler : ICommandHandler<ShopResetPasswordVerificationCheckCommand, ShopResetPasswordStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopResetPasswordVerificationCheckCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopResetPasswordStatusContract> Handle(ShopResetPasswordVerificationCheckCommand command)
        {
            var twilioAccountSid = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Account-Sid");
            var twilioAuthToken = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Auth-Token");
            var twilioEmailVerifyServiceId = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Email-Verify-Service-Id");
            
            var twilioProvider = await TwilioEmailVerificationProvider.CreateAsync(serviceProvider, new TwilioEmailVerifyServiceConfiguration(twilioAccountSid, twilioAuthToken, twilioEmailVerifyServiceId));
            var twilioEmailVerificationStatusContract = await twilioProvider.CheckVerificationEmail(command.ShopResetPasswordCreationContract
                .ConvertToTwilio().WithExecutingContext(executingRequestContextAdapter));
            
            return twilioEmailVerificationStatusContract.ConvertToShopResetPasswordStatusContract();
        }
    }
}