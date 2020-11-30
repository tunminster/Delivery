using System.Collections.Concurrent;
using Delivery.Azure.Library.Authentication.OpenIdConnect.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Delivery.Azure.Library.Authentication.ActiveDirectory.Caching.Tokens
{
    /// <summary>
    ///     A token cache store based on https://docs.microsoft.com/en-us/azure/architecture/multitenant-identity/token-cache
    ///     Dependencies:
    ///     <see cref="IConfigurationProvider" />
    ///     <see cref="IManagedCache" />
    ///     Settings:
    ///     [None]
    /// </summary>
    public class ActiveDirectoryManagedTokenCache : TokenCache
    {
        private readonly ConcurrentDictionary<string, byte[]> cachedTokens = new ConcurrentDictionary<string, byte[]>();

        public ActiveDirectoryManagedTokenCache()
        {
            BeforeAccess = BeforeAccessInternal;
            AfterAccess = AfterAccessInternal;
            LoadFromCache();
        }

        private void BeforeAccessInternal(TokenCacheNotificationArgs args)
        {
            LoadFromCache();
        }

        private void LoadFromCache()
        {
            var cacheKey = GetCacheKey();
            if (!string.IsNullOrEmpty(cacheKey) && cachedTokens.TryGetValue(cacheKey, out var cacheItem))
            {
                Deserialize(cacheItem);
            }
        }

        private static string GetCacheKey()
        {
            var claimsPrincipal = ClaimsPrincipalExtensions.GetCurrentClaimsPrincipalSafe();
            if (claimsPrincipal == null || string.IsNullOrEmpty(claimsPrincipal.Identity?.Name))
            {
                // user may not be authenticated but the service principal is being used to query the active directory graph
                return "Unset";
            }

            var clientId = claimsPrincipal.FindFirst("aud");
            return $"UserId:{claimsPrincipal.Identity.Name}::ClientId:{clientId}";
        }

        private void AfterAccessInternal(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                var cacheKey = GetCacheKey();

                if (!string.IsNullOrEmpty(cacheKey))
                {
                    if (Count > 0)
                    {
                        var target = Serialize();
                        cachedTokens.TryAdd(cacheKey, target);
                    }
                    else
                    {
                        // ReSharper disable once UnusedVariable
                        cachedTokens.TryRemove(cacheKey, out var target);
                    }

                    HasStateChanged = false;
                }
            }
        }
    }
}