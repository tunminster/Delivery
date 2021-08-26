using Microsoft.VisualStudio.Threading;

namespace Delivery.Database.Models
{
    public record ClaimData(string ClaimType, string ClaimValue)
    {
        public static readonly ClaimData JwtClaimIdentifyClaim = new("Role", "api_access");
        public static readonly ClaimData OrderPageAccess = new ("OrderPageAccess", "Allow");
        public static readonly ClaimData BackEndUserAccess = new ("OrderUpdateAccess", "Allow");
        public static readonly ClaimData ShopApiAccess = new("ShopApiAccess", "shop_api_access");

    }
}