using System.Collections.Generic;
using System.Linq;
using Delivery.Database.Entities;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Shop.Domain.Converters.ShopProfile
{
    public static class ShopProfileConverter
    {
        public static ShopProfileContract ConvertToContract(this Database.Entities.Store store)
        {
            var shopProfileContract = new ShopProfileContract
            {
                StoreId = store.ExternalId,
                StoreName = store.StoreName,
                StoreTypeId = store.StoreType.ExternalId,
                AddressLine1 = store.AddressLine1,
                AddressLine2 = store.AddressLine2,
                City = store.City,
                County = store.County,
                ImageUri = store.ImageUri,
                Radius = store.Radius,
                StoreOpeningHours = store.OpeningHours.Where(x => x.IsDeleted == false).ToList().ConvertToOpeningHours()
            };

            return shopProfileContract;
        }

        public static List<StoreOpeningHourCreationContract> ConvertToOpeningHours(this List<OpeningHour> openingHours)
        {
            var storeOpeningHourCreationList = new List<StoreOpeningHourCreationContract>();

            foreach (var item in openingHours)
            {
                var storeOpeningHourCreationContract = new StoreOpeningHourCreationContract
                {
                    DayOfWeek = item.DayOfWeek,
                    Open = item.Open,
                    Close = item.Close,
                    TimeZone = item.TimeZone
                };
                storeOpeningHourCreationList.Add(storeOpeningHourCreationContract);
            }

            return storeOpeningHourCreationList;
        }
    }
}