namespace Delivery.Azure.Library.Sharding.Interfaces
{
    /// <summary>
    ///     A shard is a way to conceptually split databases or other resources into physically separated shards which share a
    ///     common schema
    /// </summary>
    public interface IShard
    {
        /// <summary>
        ///     Key to shard on
        /// </summary>
        string Key { get; }
    }
}