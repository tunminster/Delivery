using Delivery.User.Domain.Contracts.V1.RestContracts.Managements;

namespace Delivery.User.Domain.Converters
{
    public static class UserProfileContractConverter
    {
        public static UserProfileContract ConvertToUserProfileContract(this Database.Entities.StoreUser storeUser)
        {
            var userProfileContract = new UserProfileContract
            {
                EmailAddress = storeUser.Username,
                Name = storeUser.Username,
                DateCreated = storeUser.InsertionDateTime
            };

            return userProfileContract;

        }
    }
}