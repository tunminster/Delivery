using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.Contracts;
using Delivery.Library.Twilio.Interfaces;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace Delivery.Library.Twilio.EmailVerifications
{
    public class TwilioEmailVerificationProvider : IEmailVerificationProvider
    {
        private readonly IServiceProvider serviceProvider;

        public TwilioEmailVerificationProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public static async Task<TwilioEmailVerificationProvider> CreateAsync(IServiceProvider serviceProvider,
            TwilioEmailVerifyServiceConfiguration twilioEmailVerifyServiceConfiguration)
        {
            TwilioClient.Init(twilioEmailVerifyServiceConfiguration.AccountSid,
                twilioEmailVerifyServiceConfiguration.AuthToken);

            EmailVerifyServiceConfiguration = twilioEmailVerifyServiceConfiguration;

            return await Task.FromResult(new TwilioEmailVerificationProvider(serviceProvider));
        }
        
        private static TwilioEmailVerifyServiceConfiguration? EmailVerifyServiceConfiguration { get; set; }
        
        public async Task<TwilioEmailVerificationStatusContract> SendVerificationEmail(TwilioEmailVerificationContract twilioEmailVerificationContract)
        {
             const string dependencyName = "TwilioEmailVerification";
             var dependencyData = new DependencyData("Request", twilioEmailVerificationContract.ConvertToJson());
             const string dependencyTarget = "Twilio_Email_Verify_Service";

             var verificaiton = await VerificationResource.CreateAsync(
                 to: twilioEmailVerificationContract.Email,
                 channel: "email",
                 pathServiceSid: EmailVerifyServiceConfiguration?.AccountSid);

             return verificaiton.ConvertToJson().ConvertFromJson<TwilioEmailVerificationStatusContract>();
        }
    }
}