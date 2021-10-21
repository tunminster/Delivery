using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Category.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Category creation contract
    /// </summary>
    public record CategoryCreationContract
    {
        public string Id { get; init; } = string.Empty;

        public string CategoryName { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public int ParentCategoryId { get; init; }

        public string StoreId { get; init; } = string.Empty;
        
        public int Order { get; init; }

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