namespace Delivery.Azure.Library.Core
{
    public static class HttpHeaders
    {
        public const string CorrelationId = "Request-Id";
        public const string Ring = "X-Ring";
        public const string Shard = "X-Shard";
        public const string OnBehalfOfShard = "X-Shard-On-Behalf-Of";
        public const string UserEmail = "X-User-Email";
        public const string UserRole = "X-User-Role";
        public const string UserId = "X-User-Id";
        public const string UserRequiresOAuthToken = "X-User-Requires-OAuth-Token";
        public const string SubscriptionKey = "Subscription-Key";
        public const string OcpSubscriptionKey = "Ocp-Apim-Subscription-Key";
    }
}