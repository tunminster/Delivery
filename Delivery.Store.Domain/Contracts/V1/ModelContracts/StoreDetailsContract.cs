using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Category.Domain.Contracts;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    [DataContract]
    public class StoreDetailsContract : StoreContract
    {
        [DataMember]
        public List<StoreCategoriesContract> StoreCategoriesList { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreCategoriesList)}: {StoreCategoriesList.Format()}";

        }
    }
}