namespace Delivery.User.Domain.Contracts.Facebook
{
    public class FacebookAuthorizationTokenContract
    {
        public TokenResource AccessToken { get; set; }
        public TokenResource RefreshToken { get; set; }
    }
}