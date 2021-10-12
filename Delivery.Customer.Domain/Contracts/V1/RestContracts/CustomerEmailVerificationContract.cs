namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Customer email verification contract
    /// </summary>
    public class CustomerEmailVerificationContract
    {
        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        ///  Verify code
        /// </summary>
        public string Code { get; init; } = string.Empty;
    }
}