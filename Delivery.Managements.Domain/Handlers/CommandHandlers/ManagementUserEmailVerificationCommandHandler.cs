using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.EmailVerifications;
using Delivery.Library.Twilio.Extensions;
using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using Delivery.Managements.Domain.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Managements.Domain.Handlers.CommandHandlers
{
    public record ManagementUserEmailVerificationCommand(
        ManagementUserEmailVerificationContract ManagementUserEmailVerificationContract);
    public class ManagementUserEmailVerificationCommandHandler : ICommandHandler<ManagementUserEmailVerificationCommand, ManagementUserEmailVerificationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ManagementUserEmailVerificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ManagementUserEmailVerificationStatusContract> Handle(ManagementUserEmailVerificationCommand command)
        {
            var twilioAccountSid = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Account-Sid");
            var twilioAuthToken = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Auth-Token");
            var twilioEmailVerifyServiceId = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Email-Verify-Service-Id");
            
            var twilioProvider = await TwilioEmailVerificationProvider.CreateAsync(serviceProvider, new TwilioEmailVerifyServiceConfiguration(twilioAccountSid, twilioAuthToken, twilioEmailVerifyServiceId));
            var twilioEmailVerificationStatusContract = await twilioProvider.CheckVerificationEmail(command.ManagementUserEmailVerificationContract
                .ConvertToTwilio().WithExecutingContext(executingRequestContextAdapter));

            return twilioEmailVerificationStatusContract.ConvertToManagementUserEmailVerificationStatusContract();
            
        }
    }
}