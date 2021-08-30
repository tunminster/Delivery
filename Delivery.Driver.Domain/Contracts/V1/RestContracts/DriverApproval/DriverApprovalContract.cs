namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval
{
    /// <summary>
    ///  Driver approval contract
    /// </summary>
    public record DriverApprovalContract
    {
        /// <summary>
        ///  Username
        /// </summary>
        public string Username { get; init; } = string.Empty;
    }
}