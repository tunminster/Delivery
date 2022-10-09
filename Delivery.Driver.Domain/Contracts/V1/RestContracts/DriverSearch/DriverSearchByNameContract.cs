namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverSearch
{
    public record DriverSearchByNameContract
    {
        /// <summary>
        ///  Free text search
        /// </summary>
        public string FreeTextSearch { get; init; } = string.Empty;

        /// <summary>
        ///  Current page
        /// </summary>
        public int Page { get; init; }

        /// <summary>
        ///  Page size
        /// </summary>
        public int PageSize { get; init; }
    }
}