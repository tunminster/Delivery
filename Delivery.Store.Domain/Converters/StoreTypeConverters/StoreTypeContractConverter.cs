using System.Collections.Generic;
using System.Linq;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Converters.StoreTypeConverters
{
    public static class StoreTypeContractConverter
    {
        public static List<StoreTypeContract> Convert(List<Database.Entities.StoreType> storeTypes)
        {
            var storeTypeContractList = storeTypes.Select(x => new StoreTypeContract
            {
                StoreTypeId = x.ExternalId,
                StoreTypeName = x.StoreTypeName,
                ImageUri = x.ImageUri
            }).ToList();
            
            return storeTypeContractList;
        }
    }
}