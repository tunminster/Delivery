namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment
{
    /// <summary>
    ///  Driver index creation contract
    /// </summary>
    public record DriverIndexCreationContract
    {
        public string DriverId { get; init; } = string.Empty;
    }
}