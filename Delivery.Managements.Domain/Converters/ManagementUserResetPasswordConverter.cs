using Delivery.Library.Twilio.Contracts;
using Delivery.Managements.Domain.Constants;
using Delivery.Managements.Domain.Contracts.V1.RestContracts;
using Delivery.Managements.Domain.Contracts.V1.RestContracts.ResetPassword;

namespace Delivery.Managements.Domain.Converters
{
    public static class ManagementUserResetPasswordConverter
    {
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this UserManagementResetPasswordRequestContract userManagementResetPasswordRequestContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = string.Empty,
                Email = userManagementResetPasswordRequestContract.Email,
                FromName = ManagementConstant.ManagementUserResetPasswordName,
                VerificationName = ManagementConstant.ManagementUserResetPasswordVerification,
                Subject = ManagementConstant.ManagementUserResetPasswordVerification
            };

            return twilioEmailVerificationContract;
        }
        
        public static UserManagementResetPasswordStatusContract ConvertToUserManagementResetPasswordStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var userManagementResetPasswordStatusContract = new UserManagementResetPasswordStatusContract
            {
                Email = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return userManagementResetPasswordStatusContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this UserManagementResetPasswordCreationContract userManagementResetPasswordCreationContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = userManagementResetPasswordCreationContract.Email,
                Code = userManagementResetPasswordCreationContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }
    }
}