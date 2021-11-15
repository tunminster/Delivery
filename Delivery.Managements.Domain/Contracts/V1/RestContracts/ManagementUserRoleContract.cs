namespace Delivery.Managements.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Management user role contract
    /// </summary>
    public record ManagementUserRoleContract
    {
        /// <summary>
        ///  Role
        /// </summary>
        /// <example>{{role}}</example>
        public string Role { get; init; } = string.Empty;
    }
}