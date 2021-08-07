using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Converters;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.EmailVerifications;
using Delivery.Library.Twilio.Extensions;
using Delivery.Library.Twilio.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverEmailVerification
{
    public record DriverStartEmailVerificationCommand(DriverStartEmailVerificationContract DriverStartEmailVerificationContract);
    
    public class DriverStartEmailVerificationCommandHandler : ICommandHandler<DriverStartEmailVerificationCommand, DriverEmailVerificationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverStartEmailVerificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        public async Task<DriverEmailVerificationStatusContract> Handle(DriverStartEmailVerificationCommand command)
        {
            var twilioAccountSid = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Account-Sid");
            var twilioAuthToken = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Auth-Token");
            var twilioEmailVerifyServiceId = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Twilio-{executingRequestContextAdapter.GetShard().Key}-Email-Verify-Service-Id");
            
            var twilioProvider = await TwilioEmailVerificationProvider.CreateAsync(serviceProvider, new TwilioEmailVerifyServiceConfiguration(twilioAccountSid, twilioAuthToken, twilioEmailVerifyServiceId));
            var twilioEmailVerificationStatusContract = await twilioProvider.SendVerificationEmail(command.DriverStartEmailVerificationContract
                .ConvertToTwilio().WithExecutingContext(executingRequestContextAdapter));


            return twilioEmailVerificationStatusContract.ConvertToDriverEmailVerificationStatusContract();
        }
    }
}