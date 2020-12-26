using System.Threading.Tasks;

namespace Delivery.Azure.Library.Connection.Managers.Interfaces
{
    // <summary>
    ///     Provides an encapsulation of a class which handles and creates connections, allowing for global caching of
    ///     connection information
    /// </summary>
    public interface IConnectionManager<TConnection> where TConnection : IConnection
    {
        /// <summary>
        ///     The total numbers of different connection objects (partitions) provided by the manager
        /// </summary>
        int PartitionCount { get; }

        /// <summary>
        ///     Gets a connection. The underlying manager should create one if there are none available in the pool
        /// </summary>
        /// <param name="entityName">Allows to split a single manager into different conceptual keys e.g. queue name</param>
        /// <param name="secretName">Name of the secret that resembles the connection string</param>
        /// <param name="partition">Allows to specify an exact partition to use. Must be less than or equal to the partition count</param>
        /// <returns>The connection ready to use</returns>
        Task<TConnection> GetConnectionAsync(string entityName, string secretName, int? partition = null);

        /// <summary>
        ///     Releases a connection.
        /// </summary>
        /// <param name="connectionMetadata">Metadata describing the connection</param>
        Task ReleaseConnectionAsync(IConnectionMetadata connectionMetadata);

        /// <summary>
        ///     Renews a connection
        /// </summary>
        /// <param name="connectionMetadata">Metadata describing the connection</param>
        /// <returns>The connection ready to use</returns>
        Task<TConnection> RenewConnectionAsync(IConnectionMetadata connectionMetadata);
    }
}