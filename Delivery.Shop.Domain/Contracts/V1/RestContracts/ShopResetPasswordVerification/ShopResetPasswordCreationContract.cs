namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopResetPasswordVerification
{
    /// <summary>
    ///  Shop password reset creation 
    /// </summary>
    public record ShopResetPasswordCreationContract
    {
        /// <summary>
        ///  Email
        /// </summary>
        public string Email { get; init; } = string.Empty;
        
        /// <summary>
        ///  Otp code
        /// </summary>
        public string Code { get; init; } = string.Empty;

        /// <summary>
        ///  Password 
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        ///  Confirm password
        /// </summary>
        public string ConfirmPassword { get; init; } = string.Empty;
    }
}