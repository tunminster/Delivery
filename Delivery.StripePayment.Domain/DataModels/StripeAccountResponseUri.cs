namespace Delivery.StripePayment.Domain.DataModels
{
    public record StripeAccountResponseUri(string refreshUrl, string returnUrl)
    {
        public static readonly StripeAccountResponseUri Url = new("https://www.example.com/reauth",
            "https://www.example.com/return");
    }
}