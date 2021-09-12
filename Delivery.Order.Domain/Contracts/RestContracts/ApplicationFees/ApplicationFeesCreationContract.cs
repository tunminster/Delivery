namespace Delivery.Order.Domain.Contracts.RestContracts.ApplicationFees
{
    /// <summary>
    ///  Request application fees by sub total
    /// </summary>
    public record ApplicationFeesCreationContract
    {
        /// <summary>
        ///  Subtotal amount
        /// <example>1500</example>
        /// </summary>
        public int SubTotal { get; init; }
    }
}