using System.Linq;

namespace Delivery.Domain.Helpers
{
    public static class FormatAddressLinesHelper
    {
        public static string FormatAddress(string addressLine1, string addressLine2, string city, string county,
            string country, string postalCode)
        {
            var storeProperties = new[]
            {
                addressLine1,
                addressLine2,
                city,
                county,
                country,
                postalCode.Replace(" ", "")
            };

            var formattedAddress = string.Join("+", storeProperties.Where(x => !string.IsNullOrEmpty(x)));

            return formattedAddress;
        }
    }
}