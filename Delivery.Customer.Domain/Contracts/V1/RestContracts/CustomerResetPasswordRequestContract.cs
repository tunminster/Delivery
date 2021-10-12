namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Request password reset contract
    /// </summary>
    public record CustomerResetPasswordRequestContract
    {
        /// <summary>
        ///  Reset email
        /// </summary>
        public string Email { get; init; } = string.Empty;
    }
}