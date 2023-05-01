namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval
{
    /// <summary>
    ///  Driver approval contract
    /// </summary>
    public record  DriverApprovalContract
    {
        /// <summary>
        ///  Username
        /// </summary>
        /// <example>{{userName}}</example>
        public string Username { get; init; } = string.Empty;
        
        /// <summary>
        ///  Approve
        /// </summary>
        /// <example>{{approve}}</example>
        public bool Approve { get; init; }
    }
}