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
        /// <example>{{userName}}</example>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        ///  Password
        /// </summary>
        /// <example>{{password}}</example>
        public string Password { get; init; } = string.Empty;
    }
}