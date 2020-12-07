using System.Threading.Tasks;

namespace Delivery.Azure.Library.Configuration.Configurations.Interfaces
{
    public interface ICachedSecretProvider : ISecretProvider
    {
        /// <summary>
        ///     Removes secrets from the cache so that next retrieval uses the latest version
        /// </summary>
        /// <param name="secretName">If set then only the individual secret is removed, otherwise all secrets are cleared</param>
        Task ClearCacheAsync(string? secretName = null);
    }
}