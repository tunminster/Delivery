namespace Delivery.Azure.Library.Sharding.Interfaces
{
    /// <summary>
    ///     Allows to determine if a shard should be considered internal
    /// </summary>
    public interface IInternalShardSettings
    {
        /// <summary>
        ///     Returns <c>True</c> if the shard is allowed access to the internal api for development/testing where there is no
        ///     api management to enforce this
        /// </summary>
        bool IsInternalShard(IShard shard);

        /// <summary>
        ///     Returns a default shard key for development/testing use which should always be available
        /// </summary>
        string DefaultShardKey { get; }

        /// <summary>
        ///     Returns a default shard name for development/testing use which should always be available
        /// </summary>
        string DefaultShardName { get; }
    }
}