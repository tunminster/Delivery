using System.Threading.Tasks;
using Delivery.Library.Twilio.Contracts;
using Twilio.Rest.Verify.V2.Service;

namespace Delivery.Library.Twilio.Interfaces
{
    public interface IEmailVerificationProvider
    {
        /// <summary>
        ///  Send verification email
        /// </summary>
        /// <param name="twilioEmailVerificationContract"></param>
        /// <returns></returns>
        Task<TwilioEmailVerificationStatusContract> SendVerificationEmail(TwilioEmailVerificationContract twilioEmailVerificationContract);

        /// <summary>
        ///  Check verification code
        /// </summary>
        /// <param name="twilioCheckEmailVerificationContract"></param>
        /// <returns></returns>
        Task<TwilioEmailVerificationStatusContract> CheckVerificationEmail(
            TwilioCheckEmailVerificationContract twilioCheckEmailVerificationContract);
    }
}