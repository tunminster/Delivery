namespace Delivery.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Entity update contract
    ///  Use when need to update entity from different domain
    /// </summary>
    public record EntityUpdateContract
    {
        /// <summary>
        ///  index id
        /// </summary>
        /// <example>{{id}}</example>
        public string Id { get; init; }
    }
}