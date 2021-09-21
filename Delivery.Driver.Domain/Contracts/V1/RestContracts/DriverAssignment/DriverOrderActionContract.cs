using Delivery.Database.Enums;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment
{
    /// <summary>
    ///  Driver order action contract
    /// </summary>
    public record DriverOrderActionContract
    {
        /// <summary>
        ///  Driver order status
        /// </summary>
        public DriverOrderStatus DriverOrderStatus { get; init; }

        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Reason
        /// </summary>
        public string Reason { get; init; } = string.Empty;
    }
}