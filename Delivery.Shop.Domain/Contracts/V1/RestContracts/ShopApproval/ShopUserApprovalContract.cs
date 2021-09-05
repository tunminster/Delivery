namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval
{
    /// <summary>
    ///  Shop user apprval contract
    /// </summary>
    public record ShopUserApprovalContract
    {
        /// <summary>
        ///  Email address
        /// <example>{{email}}</example>
        /// </summary>
        public string Email { get; init; } = string.Empty;
    } 
}