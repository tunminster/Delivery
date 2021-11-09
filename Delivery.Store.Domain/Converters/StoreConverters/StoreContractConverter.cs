using System.Collections.Generic;
using System.Linq;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Nest;

namespace Delivery.Store.Domain.Converters.StoreConverters
{
    public static class StoreContractConverter
    {
        public static List<StoreContract> Convert(List<Database.Entities.Store> stores)
        {
            var storeContractList = stores.Select(x => new StoreContract
            {
                StoreId = x.ExternalId,
                StoreName = x.StoreName,
                ImageUri = x.ImageUri,
                AddressLine1 = x.AddressLine1,
                AddressLine2 = x.AddressLine2,
                City = x.City,
                County = x.County,
                Country = x.Country,
                PostalCode = x.PostalCode,
                StoreType = x.StoreType?.StoreTypeName,
                StorePaymentAccountNumber = x.StorePaymentAccount?.AccountNumber ?? string.Empty,
                StoreOpeningHours = x.OpeningHours.Select(op => new StoreOpeningHourContract
                {
                    DayOfWeek = op.DayOfWeek,
                    Open =  op.Open,
                    Close = op.Close
                }).ToList(),
                Location =  new GeoLocation(x.Latitude ?? 0, x.Longitude ?? 0)
            }).ToList();
            
            return storeContractList;
        }
        
        public static StoreContract ConvertStoreContract(this Database.Entities.Store store)
        {
            var storeContract = new StoreContract
            {
                StoreId = store.ExternalId,
                StoreName = store.StoreName,
                ImageUri = store.ImageUri,
                AddressLine1 = store.AddressLine1,
                AddressLine2 = store.AddressLine2,
                City = store.City,
                County = store.County,
                Country = store.Country,
                PostalCode = store.PostalCode,
                StoreType = store.StoreType?.StoreTypeName!,
                StorePaymentAccountNumber = store.StorePaymentAccount?.AccountNumber ?? string.Empty,
                StoreOpeningHours = store.OpeningHours.Select(op => new StoreOpeningHourContract
                {
                    DayOfWeek = op.DayOfWeek,
                    Open = op.Open,
                    Close = op.Close
                }).ToList(),
                Location = new GeoLocation(store.Latitude ?? 0, store.Longitude ?? 0)
            };
            
            return storeContract;
        }
    }
}