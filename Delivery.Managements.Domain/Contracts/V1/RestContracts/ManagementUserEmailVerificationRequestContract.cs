namespace Delivery.Managements.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Management user email verification contract
    /// </summary>
    public record ManagementUserEmailVerificationRequestContract
    {
        /// <summary>
        ///  Full name
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;
    }
}