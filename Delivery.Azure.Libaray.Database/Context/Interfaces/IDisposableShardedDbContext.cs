using System;

namespace Delivery.Azure.Libaray.Database.Context.Interfaces
{
    /// <summary>
    ///  Enforces disposal for sharded database contexts
    /// </summary>
    public interface IDisposableShardedDbContext : IAsyncDisposable
    {
        /// <summary>
        ///  Indication whether or not the context is disposed
        /// </summary>
        bool IsDisposed { get; }
    }
}