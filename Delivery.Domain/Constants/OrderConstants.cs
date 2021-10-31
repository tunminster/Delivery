namespace Delivery.Domain.Constants
{
    public static class OrderConstants
    {
        public const string ServiceBusConnectionStringName = "ServiceBus-Topic-Orders-ConnectionString";
        public const string ServiceBusEntityName = "orders";
        
        public const string TaxRateCountry = "United States";
        public const string TaxRateContainer = "country-taxes";
    }
}