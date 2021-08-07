using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Azure.Library.Telemetry.Constants;
using Delivery.Library.Twilio.Configurations;
using Delivery.Library.Twilio.Contracts;
using Delivery.Library.Twilio.Converters;
using Delivery.Library.Twilio.Extensions;
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
             
             var telemetryContextProperties = new Dictionary<string, string>
             {
                 {"Body", twilioEmailVerificationContract.ConvertToJson()},
                 {CustomProperties.CorrelationId, twilioEmailVerificationContract.GetCorrelationId() ?? string.Empty},
                 {CustomProperties.Shard, twilioEmailVerificationContract.GetShard().Key},
                 {CustomProperties.Ring, twilioEmailVerificationContract.GetRing()?.ToString() ?? "Unknown"}
             };
             
             var channelConfiguration = new Dictionary<string, object>
             {
                 {"substitutions", new Dictionary<string, object>
                 {
                     {"username", twilioEmailVerificationContract.Username},
                     {"application-name", ""}
                 }}
             };
             
             var verificationResource = await new DependencyMeasurement(serviceProvider)
                 .ForDependency(dependencyName, MeasuredDependencyType.AzureServiceBus, dependencyData.ConvertToJson(), dependencyTarget)
                 .WithContextualInformation(telemetryContextProperties)
                 .TrackAsync(async () => await VerificationResource.CreateAsync(
                     channelConfiguration: channelConfiguration,
                     to: twilioEmailVerificationContract.Email,
                     channel: "email",
                     pathServiceSid: EmailVerifyServiceConfiguration?.EmailVerifyServiceId));
             
             return verificationResource.ConvertToTwilioEmailVerificationStatusContract();
        }

        public async Task<TwilioEmailVerificationStatusContract> CheckVerificationEmail(TwilioCheckEmailVerificationContract twilioCheckEmailVerificationContract)
        {
            const string dependencyName = "TwilioCheckEmailVerification";
            var dependencyData = new DependencyData("Request", twilioCheckEmailVerificationContract.ConvertToJson());
            const string dependencyTarget = "Twilio_Email_Verify_Service";
            
            var telemetryContextProperties = new Dictionary<string, string>
            {
                {"Body", twilioCheckEmailVerificationContract.ConvertToJson()},
                {CustomProperties.CorrelationId, twilioCheckEmailVerificationContract.GetCorrelationId() ?? string.Empty},
                {CustomProperties.Shard, twilioCheckEmailVerificationContract.GetShard().Key},
                {CustomProperties.Ring, twilioCheckEmailVerificationContract.GetRing()?.ToString() ?? "Unknown"}
            };
            
            var verificationResource = await new DependencyMeasurement(serviceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.AzureServiceBus, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(telemetryContextProperties)
                .TrackAsync(async () => await VerificationCheckResource.CreateAsync(
                    to: twilioCheckEmailVerificationContract.Email,
                    code: twilioCheckEmailVerificationContract.Code,
                    pathServiceSid: EmailVerifyServiceConfiguration?.EmailVerifyServiceId));
             
            return verificationResource.ConvertToTwilioEmailVerificationStatusContract();
        }
    }
}