using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverResetPasswordVerification;
using Delivery.Driver.Domain.Converters;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.EmailVerifications;
using Delivery.Library.Twilio.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverCheckResetPasswordVerification
{
    public record DriverCheckResetPasswordVerificationCommand(DriverResetPasswordCreationContract DriverResetPasswordCreationContract);
    public class DriverCheckResetPasswordVerificationCommandHandler : ICommandHandler<DriverCheckResetPasswordVerificationCommand, DriverResetPasswordStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverCheckResetPasswordVerificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverResetPasswordStatusContract> HandleAsync(DriverCheckResetPasswordVerificationCommand command)
        {
            var twilioAccountSid = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Account-Sid");
            var twilioAuthToken = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Auth-Token");
            var twilioEmailVerifyServiceId = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Email-Verify-Service-Id");
            
            var twilioProvider = await TwilioEmailVerificationProvider.CreateAsync(serviceProvider, new TwilioEmailVerifyServiceConfiguration(twilioAccountSid, twilioAuthToken, twilioEmailVerifyServiceId));
            var twilioEmailVerificationStatusContract = await twilioProvider.CheckVerificationEmail(command.DriverResetPasswordCreationContract
                .ConvertToTwilio().WithExecutingContext(executingRequestContextAdapter));

            return twilioEmailVerificationStatusContract.ConvertToDriverResetPasswordStatusContract();
        }
    }
}