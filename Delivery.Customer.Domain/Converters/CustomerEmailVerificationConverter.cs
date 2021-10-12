using Delivery.Customer.Domain.Constants;
using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using Delivery.Library.Twilio.Contracts;

namespace Delivery.Customer.Domain.Converters
{
    public static class CustomerEmailVerificationConverter
    {
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this CustomerEmailVerificationRequestContract customerEmailVerificationRequestContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = customerEmailVerificationRequestContract.FullName,
                Email = customerEmailVerificationRequestContract.Email,
                FromName = CustomerConstant.CustomerFromName,
                VerificationName = CustomerConstant.CustomerEmailVerification,
                Subject = CustomerConstant.CustomerEmailVerification
            };

            return twilioEmailVerificationContract;
        }
        
        public static CustomerEmailVerificationStatusContract ConvertToCustomerEmailVerificationStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var driverStartEmailVerificationStatusContract = new CustomerEmailVerificationStatusContract
            {
                UserEmailAddress = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return driverStartEmailVerificationStatusContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this CustomerEmailVerificationContract customerEmailVerificationContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = customerEmailVerificationContract.Email,
                Code = customerEmailVerificationContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }
        
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this CustomerResetPasswordRequestContract customerResetPasswordRequestContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = string.Empty,
                Email = customerResetPasswordRequestContract.Email,
                FromName = CustomerConstant.CustomerResetPasswordName,
                VerificationName = CustomerConstant.CustomerResetPasswordVerification,
                Subject = CustomerConstant.CustomerResetPasswordVerification
            };

            return twilioEmailVerificationContract;
        }
        
        public static CustomerResetPasswordStatusContract ConvertToCustomerResetPasswordStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var customerResetPasswordStatusContract = new CustomerResetPasswordStatusContract
            {
                Email = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return customerResetPasswordStatusContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this CustomerResetPasswordCreationContract customerResetPasswordCreationContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = customerResetPasswordCreationContract.Email,
                Code = customerResetPasswordCreationContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }
    }
}