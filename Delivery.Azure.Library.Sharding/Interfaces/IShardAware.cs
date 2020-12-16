namespace Delivery.Azure.Library.Sharding.Interfaces
{
    /// <summary>
    ///     Any object which requires a shard to operate should expose its shard immutably for debugging and logging purposes
    /// </summary>
    public interface IShardAware
    {
        /// <summary>
        ///     The current shard which an object's context is based on
        /// </summary>
        public IShard Shard { get; }
    }
}