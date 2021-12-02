namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval
{
    /// <summary>
    ///  Shop user approval contract
    /// </summary>
    public record ShopUserApprovalContract
    {
        /// <summary>
        ///  Email address
        /// </summary>
        /// <example>{{email}}</example>
        public string Email { get; init; } = string.Empty;
        
        /// <summary>
        ///  Approve
        /// </summary>
        /// <example>{{approve}}</example>
        public bool Approve { get; init; }
    } 
}