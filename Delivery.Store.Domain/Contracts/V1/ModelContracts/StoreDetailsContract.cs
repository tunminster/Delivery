using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Category.Domain.Contracts;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    /// <summary>
    ///  Store details controct
    /// </summary>
    public record StoreDetailsContract : StoreContract
    {
        /// <summary>
        ///  Store category list
        /// </summary>
        /// <example>{{storeCategoriesList}}</example>
        public List<StoreCategoriesContract> StoreCategoriesList { get; set; } = new();
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreCategoriesList)}: {StoreCategoriesList.Format()}";

        }
    }
}