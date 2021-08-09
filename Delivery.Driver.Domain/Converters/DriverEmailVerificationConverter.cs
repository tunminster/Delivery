using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverCheckEmailVerification;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverResetPasswordVerification;
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
        
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this DriverResetPasswordRequestContract driverResetPasswordRequestContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = string.Empty,
                Email = driverResetPasswordRequestContract.Email
            };

            return twilioEmailVerificationContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this DriverCheckEmailVerificationContract driverCheckEmailVerificationContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = driverCheckEmailVerificationContract.Email,
                Code = driverCheckEmailVerificationContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this DriverResetPasswordCreationContract driverResetPasswordCreationContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = driverResetPasswordCreationContract.Email,
                Code = driverResetPasswordCreationContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }

        public static DriverEmailVerificationStatusContract ConvertToDriverEmailVerificationStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var driverStartEmailVerificationStatusContract = new DriverEmailVerificationStatusContract
            {
                UserEmailAddress = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return driverStartEmailVerificationStatusContract;
        }
        
        public static DriverResetPasswordStatusContract ConvertToDriverResetPasswordStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var driverResetPasswordStatusContract = new DriverResetPasswordStatusContract
            {
                Email = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return driverResetPasswordStatusContract;
        }
    }
}