namespace Delivery.Managements.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Management user contract
    /// </summary>
    /// <remarks>This contract allows to add user to admin role</remarks>
    public record ManagementUserContract
    {
        /// <summary>
        ///  Email address
        /// </summary>
        /// <example>{{emailAddress}}</example>
        public string Email { get; init; } = string.Empty;
    }
}