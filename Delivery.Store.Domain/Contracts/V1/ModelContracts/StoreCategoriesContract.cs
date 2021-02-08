using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;
using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    [DataContract]
    public class StoreCategoriesContract : CategoryContract
    {
        [DataMember]
        public List<ProductContract> Products { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Products)}: {Products.Format()}";

        }
    }
}