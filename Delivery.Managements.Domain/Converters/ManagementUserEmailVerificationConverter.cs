using Delivery.Library.Twilio.Contracts;
using Delivery.Managements.Domain.Constants;
using Delivery.Managements.Domain.Contracts.V1.RestContracts;

namespace Delivery.Managements.Domain.Converters
{
    public static class ManagementUserEmailVerificationConverter
    {
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this ManagementUserEmailVerificationRequestContract managementUserEmailVerificationRequestContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = managementUserEmailVerificationRequestContract.FullName,
                Email = managementUserEmailVerificationRequestContract.Email,
                FromName = ManagementConstant.ManagementFromName,
                VerificationName = ManagementConstant.ManagementEmailVerification,
                Subject = ManagementConstant.ManagementEmailVerification
            };

            return twilioEmailVerificationContract;
        }
        
        public static ManagementUserEmailVerificationStatusContract ConvertToManagementUserEmailVerificationStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var driverStartEmailVerificationStatusContract = new ManagementUserEmailVerificationStatusContract
            {
                UserEmailAddress = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return driverStartEmailVerificationStatusContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this ManagementUserEmailVerificationContract managementUserEmailVerificationContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = managementUserEmailVerificationContract.Email,
                Code = managementUserEmailVerificationContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }
    }
}