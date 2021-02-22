using System.IO;

namespace Delivery.Azure.Library.Storage.Blob.Models
{
    public record BlobParametersUploadModel(BlobParametersRequestModel BlobParametersRequestModel, Stream Stream);
}