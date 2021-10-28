namespace Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement
{
    /// <summary>
    ///  Product management update contract
    /// </summary>
    public record ProductManagementUpdateContract : ProductManagementCreationContract
    {
        /// <summary>
        ///  Product id
        /// </summary>
        /// <example>{{productId}}</example>
        public string ProductId { get; init; } = string.Empty;
    }
}