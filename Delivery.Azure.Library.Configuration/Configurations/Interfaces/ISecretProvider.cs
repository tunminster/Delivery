using System;
using System.Threading.Tasks;

namespace Delivery.Azure.Library.Configuration.Configurations.Interfaces
{
    /// <summary>
    ///     Manages access to secrets stored in key vault
    /// </summary>
    /// <remarks>
    ///     Only use this for simple applications (e.g. console application). All other applications should use
    ///     <see cref="ICachedSecretProvider" /> to prevent key vault from being overloaded with requests
    /// </remarks>
    public interface ISecretProvider
    {
        /// <summary>
        ///     Retrieves a specific secret from an Azure Key Vault
        /// </summary>
        /// <param name="secretName">Name of the secret (Cannot contain '.')</param>
        /// <returns>Value for the specified secret</returns>
        /// <exception cref="ArgumentException">Exception thrown when the specified secret name contains a '.'</exception>
        /// <exception cref="ArgumentNullException">Exception thrown when there was no secret name specified</exception>
        /// <exception cref="SecretNotFoundException">Exception thrown when the specified secret was not found</exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Exception thrown configured application is not authorized to retrieve a
        ///     secret
        /// </exception>
        Task<string> GetSecretAsync(string secretName);
    }
}