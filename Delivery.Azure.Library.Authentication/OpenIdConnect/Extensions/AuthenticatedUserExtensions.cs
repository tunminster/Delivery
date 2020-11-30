using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Sharding.Extensions;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.Authentication.OpenIdConnect.Extensions
{
    public static class AuthenticatedUserExtensions
    {
        /// <summary>
        ///     Gets the authenticated user details
        /// </summary>
        public static AuthenticatedUserContract GetAuthenticatedUser(this HttpRequest httpRequest, string defaultRole)
        {
            var userEmail = httpRequest.GetUserEmail();
            var shard = httpRequest.GetShard();

            var user = httpRequest.HttpContext.User;
            if (user.Identity?.IsAuthenticated ?? false)
            {
                // use the email of the user authenticated via Okta
                userEmail = user.GetEmail();

                // use the role of the user provided by Okta
                defaultRole = user.GetRole();

                // get the tenant the user belongs to
                shard = user.GetShard();
            }

            var authenticatedUser = new AuthenticatedUserContract
            {
                UserEmail = userEmail,
                Role = defaultRole,
                ShardKey = shard.Key
            };

            return authenticatedUser;
        }
    }
}