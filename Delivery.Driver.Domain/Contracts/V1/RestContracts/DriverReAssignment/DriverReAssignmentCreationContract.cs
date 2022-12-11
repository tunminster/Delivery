namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment
{
    /// <summary>
    ///  Driver reAssignment creation contract
    /// </summary>
    public record DriverReAssignmentCreationContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
    }
}