namespace Delivery.Managements.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Management login contract
    /// </summary>
    public record ManagementUserLoginContract
    {
        /// <summary>
        ///  Username
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        ///  Password
        /// </summary>
        public string Password { get; init; } = string.Empty;
    }
}