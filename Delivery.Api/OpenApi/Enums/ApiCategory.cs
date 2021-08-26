namespace Delivery.Api.OpenApi.Enums
{
    /// <summary>
    ///  Api category
    /// </summary>
    public enum ApiCategory
    {
        /// <summary>
        ///  Contains public apis which have driver use-cases
        /// </summary>
        Driver,
        
        /// <summary>
        ///  Contains management apis 
        /// </summary>
        Management,
        
        /// <summary>
        ///  Contains store owner apis which have store owner use-cases
        /// </summary>
        ShopOwner,
        
        /// <summary>
        ///  Contains customer apis which have customer use-cases
        /// </summary>
        Customer
    }
}