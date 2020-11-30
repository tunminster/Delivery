using System;
using System.Linq;
using System.Security.Claims;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Microsoft.IdentityModel.Tokens;

namespace Delivery.Azure.Library.Authentication.OpenIdConnect.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public const string RoleClaimName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public static ClaimsPrincipal? GetCurrentClaimsPrincipalSafe()
        {
            try
            {
                return ClaimsPrincipal.Current;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public static string GetName(this ClaimsPrincipal? claimsPrincipal)
        {
            var identityName = claimsPrincipal?.Identity?.Name;
            if (string.IsNullOrEmpty(identityName))
            {
                throw new SecurityTokenInvalidIssuerException($"Claims principal does not have expected name. Principal: {claimsPrincipal}");
            }

            return identityName;
        }

        public static string GetEmail(this ClaimsPrincipal? claimsPrincipal)
        {
            var claimName = "preferred_username";
            return GetRoleValue(claimsPrincipal, claimName);
        }

        public static IShard GetShard(this ClaimsPrincipal? claimsPrincipal)
        {
            var claimName = "groups";
            var roleValue = GetRoleValue(claimsPrincipal, claimName);
            return Shard.Create(roleValue);
        }

        public static string GetRole(this ClaimsPrincipal? claimsPrincipal)
        {
            return GetRoleValue(claimsPrincipal, RoleClaimName);
        }

        private static string GetRoleValue(this ClaimsPrincipal? claimsPrincipal, string claimName)
        {
            var identityName = claimsPrincipal?.Claims.SingleOrDefault(claim => claim.Type == claimName);
            if (identityName == null)
            {
                throw new SecurityTokenInvalidIssuerException($"Claims principal does not have expected claim for key {claimName}. Principal: {claimsPrincipal}");
            }

            return identityName.Value;
        }
    }
}