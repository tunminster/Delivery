using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Category.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Category creation contract
    /// </summary>
    public record CategoryCreationContract
    {
        

        public string CategoryName { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public int ParentCategoryId { get; init; }

        public int Order { get; init; }
    }
}