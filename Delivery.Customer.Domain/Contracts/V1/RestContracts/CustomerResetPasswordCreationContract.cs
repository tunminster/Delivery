namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Customer reset password creation contract
    /// </summary>
    public record CustomerResetPasswordCreationContract
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