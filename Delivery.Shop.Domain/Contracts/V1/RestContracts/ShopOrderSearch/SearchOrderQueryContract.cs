namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderSearch
{
    /// <summary>
    ///  Search order query contract
    /// </summary>
    public record SearchOrderQueryContract
    {
        /// <summary>
        ///  Free text search
        /// </summary>
        public string FreeTextSearch { get; init; } = string.Empty;

        /// <summary>
        ///  Filter property
        /// </summary>
        /// <example>Status:Accepted</example>
        public string Filters { get; init; } = string.Empty;

        /// <summary>
        ///  Page number
        /// </summary>
        public int Page { get; init; } = 1;

        /// <summary>
        ///  Page size
        /// </summary>
        public int PageSize { get; init; } = 50;
    }
}