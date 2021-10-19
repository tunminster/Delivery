using Delivery.Library.Twilio.Contracts;
using Delivery.Shop.Domain.Constants;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopResetPasswordVerification;

namespace Delivery.Shop.Domain.Converters
{
    public static class ShopResetPasswordConverter
    {
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this ShopResetPasswordRequestContract shopResetPasswordRequestContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = string.Empty,
                Email = shopResetPasswordRequestContract.Email,
                FromName = ShopConstant.ShopResetPasswordName,
                VerificationName = ShopConstant.ShopResetPasswordVerification,
                Subject = ShopConstant.ShopResetPasswordVerification
            };

            return twilioEmailVerificationContract;
        }
        
        public static ShopResetPasswordStatusContract ConvertToShopResetPasswordStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var shopResetPasswordStatusContract = new ShopResetPasswordStatusContract
            {
                Email = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return shopResetPasswordStatusContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this ShopResetPasswordCreationContract shopResetPasswordCreationContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = shopResetPasswordCreationContract.Email,
                Code = shopResetPasswordCreationContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }
    }
}