using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using Delivery.Customer.Domain.Converters;
using Delivery.Domain.CommandHandlers;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.EmailVerifications;
using Delivery.Library.Twilio.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers
{
    public record CustomerResetPasswordVerificationCommand(
        CustomerResetPasswordCreationContract CustomerResetPasswordCreationContract);
    
    public class CustomerResetPasswordVerificationCommandHandler : ICommandHandler<CustomerResetPasswordVerificationCommand, CustomerResetPasswordStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public CustomerResetPasswordVerificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CustomerResetPasswordStatusContract> HandleAsync(CustomerResetPasswordVerificationCommand command)
        {
            var twilioAccountSid = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Account-Sid");
            var twilioAuthToken = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Auth-Token");
            var twilioEmailVerifyServiceId = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Email-Verify-Service-Id");
            
            var twilioProvider = await TwilioEmailVerificationProvider.CreateAsync(serviceProvider, new TwilioEmailVerifyServiceConfiguration(twilioAccountSid, twilioAuthToken, twilioEmailVerifyServiceId));
            var twilioEmailVerificationStatusContract = await twilioProvider.CheckVerificationEmail(command.CustomerResetPasswordCreationContract
                .ConvertToTwilio().WithExecutingContext(executingRequestContextAdapter));

            return twilioEmailVerificationStatusContract.ConvertToCustomerResetPasswordStatusContract();
        }
    }
}