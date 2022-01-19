using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Store management contract
    /// </summary>
    public record StoreManagementContract : StoreContract
    {
        /// <summary>
        ///  Store type id
        /// <example>{{storeTypeId}}</example>
        /// </summary>
        public string StoreTypeId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Is active
        /// </summary>
        /// <example>{{isActive}}</example>
        public bool IsActive { get; init; }
    }
}