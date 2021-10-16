namespace Delivery.Managements.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Management user email verification contract
    /// </summary>
    public class ManagementUserEmailVerificationContract
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