namespace Delivery.User.Domain.Contracts.Facebook
{
    public class TokenResource
    {
        public string Token { get; set; }
        public long Expiry { get; set; }
    }
}