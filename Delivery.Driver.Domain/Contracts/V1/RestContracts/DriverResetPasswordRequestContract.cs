namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Request password reset contract
    /// </summary>
    public record DriverResetPasswordRequestContract
    {
        /// <summary>
        ///  Reset email
        /// </summary>
        public string Email { get; init; } = string.Empty;
    }
}