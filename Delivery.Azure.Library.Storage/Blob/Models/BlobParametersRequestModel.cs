namespace Delivery.Azure.Library.Storage.Blob.Models
{
    public record BlobParametersRequestModel(string FileSystemName, string FileName, string BlobUri = null, string ContentType = null);
}