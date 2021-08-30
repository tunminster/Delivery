using Delivery.Library.Twilio.Contracts;
using Delivery.Shop.Domain.Constants;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopEmailVerification;

namespace Delivery.Shop.Domain.Converters
{
    public static class ShopEmailVerificationConverter
    {
        public static TwilioEmailVerificationContract ConvertToTwilio(
            this ShopEmailVerificationContract shopEmailVerificationContract)
        {
            var twilioEmailVerificationContract = new TwilioEmailVerificationContract
            {
                Username = shopEmailVerificationContract.FullName,
                Email = shopEmailVerificationContract.EmailAddress,
                FromName = ShopConstant.ShopFromName,
                VerificationName = ShopConstant.ShopEmailVerification,
                Subject = ShopConstant.ShopEmailVerification
            };

            return twilioEmailVerificationContract;
        }
        
        public static ShopEmailVerificationStatusContract ConvertToShopEmailVerificationStatusContract(
            this TwilioEmailVerificationStatusContract twilioEmailVerificationStatusContract)
        {
            var shopEmailVerificationStatusContract = new ShopEmailVerificationStatusContract
            {
                UserEmailAddress = twilioEmailVerificationStatusContract.To,
                Status = twilioEmailVerificationStatusContract.Status,
                Valid = twilioEmailVerificationStatusContract.Valid,
                DateCreated = twilioEmailVerificationStatusContract.DateCreated
            };

            return shopEmailVerificationStatusContract;
        }
        
        public static TwilioCheckEmailVerificationContract ConvertToTwilio(
            this ShopEmailVerificationCheckContract shopEmailVerificationCheckContract)
        {
            var twilioCheckEmailVerificationContract = new TwilioCheckEmailVerificationContract
            {
                Email = shopEmailVerificationCheckContract.Email,
                Code = shopEmailVerificationCheckContract.Code
            };

            return twilioCheckEmailVerificationContract;
        }
    }
}