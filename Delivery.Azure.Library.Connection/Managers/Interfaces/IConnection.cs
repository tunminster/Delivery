using System;

namespace Delivery.Azure.Library.Connection.Managers.Interfaces
{
    public interface IConnection : IAsyncDisposable
    {
        IConnectionMetadata Metadata { get; }
    }
}