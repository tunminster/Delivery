namespace Delivery.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Index creation contract
    /// </summary>
    public record IndexCreationContract
    {
        /// <summary>
        ///  index id
        /// </summary>
        /// <example>{{id}}</example>
        public string Id { get; init; }
    }
}