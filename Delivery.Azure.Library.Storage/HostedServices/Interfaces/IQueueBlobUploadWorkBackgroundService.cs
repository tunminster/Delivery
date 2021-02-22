using System.IO;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Delivery.Azure.Library.Storage.HostedServices.Interfaces
{
    /// <summary>
    ///     Allows to pass work to the hosted service to complete
    /// </summary>
    /// <remarks>Useful for fire-and-forget scenarios on hot path processing</remarks>
    public interface IQueueBlobUploadWorkBackgroundService : IQueueWorkBackgroundService
    {
        /// <summary>
        ///     Adds work to be completed asynchronously to the storage account
        /// </summary>
        Task EnqueueBackgroundWorkAsync(IExecutingRequestContextAdapter executingRequestContextAdapter, CloudBlockBlob blob, Stream stream);
    }
}