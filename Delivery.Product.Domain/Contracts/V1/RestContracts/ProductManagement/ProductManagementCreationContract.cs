namespace Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement
{
    /// <summary>
    ///  Product management creation contract
    /// </summary>
    public record ProductManagementCreationContract
    {
        /// <summary>
        ///  Product name
        /// </summary>
        /// <example>{{productName}}</example>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        ///  Description
        /// </summary>
        /// <example>{{description}}</example>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///  Product image
        /// </summary>
        /// <example>{{productImage}}</example>
        public string ProductImage { get; set; } = string.Empty;

        /// <summary>
        ///  Product image url
        /// </summary>
        /// <example>{{productImageUrl}}</example>
        public string ProductImageUrl { get; set; } = string.Empty;

        /// <summary>
        ///  Unit price
        /// </summary>
        /// <example>{{unitPrice}}</example>
        public int UnitPrice { get; set; }

        /// <summary>
        ///  Category id
        /// </summary>
        /// <example>{{categoryId}}</example>
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>
        ///  Store id
        /// </summary>
        /// <example>{{storeId}}</example>
        public string StoreId { get; set; } = string.Empty;
    }
}