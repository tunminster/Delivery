namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.Coupon
{
    /// <summary>
    ///  Coupon creation status
    /// </summary>
    public record CouponManagementCreationStatusContract
    {
        /// <summary>
        ///  Coupon code
        /// </summary>
        public string CouponCode { get; init; } = string.Empty;
    }
}