using System;
using Delivery.Library.Twilio.Contracts;
using Twilio.Rest.Verify.V2.Service;

namespace Delivery.Library.Twilio.Converters
{
    public static class TwilioVerificationResourceConverter
    {
        public static TwilioEmailVerificationStatusContract ConvertToTwilioEmailVerificationStatusContract(
            this VerificationResource verificationResource)
        {
            var twilioEmailVerificationStatusContract = new TwilioEmailVerificationStatusContract
            {
                Sid = verificationResource.Sid,
                ServiceId = verificationResource.ServiceSid,
                AccountId = verificationResource.AccountSid,
                To = verificationResource.To,
                Channel = verificationResource.Channel.ToString(),
                Status = verificationResource.Status,
                Valid = verificationResource.Valid,
                DateCreated = DateTimeOffset.Parse(verificationResource.DateCreated.ToString()!),
                DateUpdated = DateTimeOffset.Parse(verificationResource.DateUpdated.ToString()!),
                Lookup = verificationResource.Lookup,
                Amount = verificationResource.Amount,
                Payee = verificationResource.Payee,
                SendCodeAttempts = verificationResource.SendCodeAttempts,
                Url = verificationResource.Url
            };

            return twilioEmailVerificationStatusContract;
        }
        
        public static TwilioEmailVerificationStatusContract ConvertToTwilioEmailVerificationStatusContract(
            this VerificationCheckResource verificationCheckResource)
        {
            var twilioEmailVerificationStatusContract = new TwilioEmailVerificationStatusContract
            {
                Sid = verificationCheckResource.Sid,
                ServiceId = verificationCheckResource.ServiceSid,
                AccountId = verificationCheckResource.AccountSid,
                To = verificationCheckResource.To,
                Channel = verificationCheckResource.Channel.ToString(),
                Status = verificationCheckResource.Status,
                Valid = verificationCheckResource.Valid,
                DateCreated = DateTimeOffset.Parse(verificationCheckResource.DateCreated.ToString()!),
                DateUpdated = DateTimeOffset.Parse(verificationCheckResource.DateUpdated.ToString()!),
                Amount = verificationCheckResource.Amount,
                Payee = verificationCheckResource.Payee,
            };

            return twilioEmailVerificationStatusContract;
        }
    }
}