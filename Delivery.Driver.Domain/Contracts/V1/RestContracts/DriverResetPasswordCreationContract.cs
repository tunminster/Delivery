namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver password reset creation 
    /// </summary>
    public record DriverResetPasswordCreationContract
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