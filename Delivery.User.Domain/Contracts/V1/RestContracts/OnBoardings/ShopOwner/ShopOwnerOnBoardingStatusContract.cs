namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.ShopOwner
{
    /// <summary>
    ///  Shop owner on boarding status contract
    /// </summary>
    public record ShopOwnerOnBoardingStatusContract
    {
        /// <summary>
        ///  Display success message
        /// </summary>
        public string Message { get; init; } = string.Empty;
    }
}