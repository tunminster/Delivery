using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Caching.Cache.FeatureFlags
{
    [DataContract]
    public enum CachingFeatures
    {
        [EnumMember] None,

        /// <summary>
        ///     Allows to disable the redis cache
        /// </summary>
        [EnumMember] RedisCache,

        /// <summary>
        ///     Allows to disable cache flows which rely on ClearAsync for cache invalidation to be disabled
        /// </summary>
        [EnumMember] MutableCache
    }
}