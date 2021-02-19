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
            var storeTypeContractList = stores.Select(x => new StoreContract
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
                StoreType = x.StoreType.StoreTypeName,
                StoreOpeningHours = x.OpeningHours.Select(op => new StoreOpeningHourContract
                {
                    DayOfWeek = op.DayOfWeek,
                    Open =  op.Open,
                    Close = op.Close
                }).ToList(),
                Location =  new GeoLocation(x.Latitude ?? 0, x.Longitude ?? 0)
            }).ToList();
            
            return storeTypeContractList;
        }
    }
}