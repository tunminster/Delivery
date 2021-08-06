using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Library.Twilio.Contracts;

namespace Delivery.Driver.Domain.Converters
{
    public static class DriverEmailVerificationConverter
    {
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this DriverStartEmailVerificationContract driverStartEmailVerificationContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = driverStartEmailVerificationContract.FullName,
                Email = driverStartEmailVerificationContract.Email
            };

            return twilioEmailVerificationContract;
        }

        public static DriverStartEmailVerificationStatusContract ConvertToDriverStartEmailVerificationStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var driverStartEmailVerificationStatusContract = new DriverStartEmailVerificationStatusContract
            {
                UserEmailAddress = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return driverStartEmailVerificationStatusContract;
        }
    }
}