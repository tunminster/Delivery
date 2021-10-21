using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Category.Domain.Contracts.V1.RestContracts;

namespace Delivery.Category.Domain.Contracts.V1.ModelContracts
{
    public record CategoryContract : CategoryCreationContract
    {
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Id)}: {Id.Format()}," +
                   $"{nameof(CategoryName)}: {CategoryName.Format()}," +
                   $"{nameof(Description)}: {Description.Format()}," +
                   $"{nameof(ParentCategoryId)}: {ParentCategoryId.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(Order)}: {Order.Format()};";
        }
    }
}