using System;
using System.Threading;
using System.Threading.Tasks;

namespace Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces
{
    public interface IQueueWorkBackgroundService
    {
        /// <summary>
        ///     Adds work to be completed asynchronously
        /// </summary>
        bool EnqueueBackgroundWork(Func<CancellationToken, Task> workItem);

        int TasksWaiting { get; }
    }
}