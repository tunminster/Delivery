namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Customer email verification contract
    /// </summary>
    public record CustomerEmailVerificationRequestContract
    {
        /// <summary>
        ///  Driver's full name
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;
    }
}