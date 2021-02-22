using Microsoft.WindowsAzure.Storage.Blob;

namespace Delivery.Azure.Library.Storage.Blob.Models
{
    public record BlobParametersModel(BlobParametersRequestModel BlobParametersRequestModel, CloudBlockBlob CloudBlockBlob);
}