using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Monads;

namespace Delivery.Azure.Library.Caching.Cache.Interfaces
{
    /// <summary>
    ///     Represents a simple cache which supports storing and retrieving objects by partition
    /// </summary>
    public interface IManagedCache : IAsyncDisposable
    {
	    /// <summary>
	    ///     Gets an object from the cache
	    /// </summary>
	    /// <typeparam name="T">The type of object to get from the cache</typeparam>
	    /// <param name="key">The case-insensitive cache key to use</param>
	    /// <param name="partition">A logical partition to use which can be used for cache clearing and more efficient lookups</param>
	    /// <returns>The object from cache or not-present monad</returns>
	    /// <exception cref="InvalidCastException">Thrown if the expected type T does not match what is stored in the cache</exception>
	    Task<Maybe<T>> GetAsync<T>(string key, string partition);

        /// <summary>
        ///     Inserts or replaces an object into the cache
        /// </summary>
        /// <typeparam name="T">The type of object to get from the cache</typeparam>
        /// <param name="key">The case-insensitive cache key to use</param>
        /// <param name="targetValue">The target object to insert</param>
        /// <param name="partition">A logical partition to use which can be used for cache clearing and more efficient lookups</param>
        /// <param name="correlationId">The correlation id for dependency trace</param>
        /// <param name="cacheExpirySeconds">
        ///     The number of seconds from Utc now whereupon the target object will be evicted from
        ///     the cache
        /// </param>
        /// <returns>The same target object (fluent api)</returns>
        Task<Maybe<T>> AddAsync<T>(string key, T targetValue, string partition, string correlationId, int? cacheExpirySeconds = null);

        /// <summary>
        ///     Removes a specific key from a partition if set. If only partition is set then the entire partition is cleared (can
        ///     be expensive if a lot of objects exist for a given partition)
        /// </summary>
        /// <param name="partition">A logical partition to use which can be used for cache clearing and more efficient lookups</param>
        /// <param name="correlationId">The correlation id to trace the telemetry</param>
        /// <param name="key">If set then only the selected key is removed</param>
        Task ClearAsync(string partition, string correlationId, string? key = null);
    }
}