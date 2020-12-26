using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;

namespace Delivery.Azure.Library.Connection.Managers
{
    public abstract class Connection : IConnection
    {
        protected Connection(IConnectionMetadata connectionMetadata)
        {
            Metadata = connectionMetadata;
        }

        bool disposed;
        
        public virtual ValueTask DisposeAsync()
        {
            if (disposed)
            {
                return new ValueTask();
            }

            disposed = true;
            return new ValueTask();
        }

        public IConnectionMetadata Metadata { get; }
    }
}