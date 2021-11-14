using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.WebApi.Files
{
    public class DisposableFormFile : FormFile, IAsyncDisposable
    {
        private readonly Stream baseStream;

        public DisposableFormFile(Stream baseStream, long baseStreamOffset, long length, string name, string fileName) : base(baseStream, baseStreamOffset, length, name, fileName)
        {
            this.baseStream = baseStream;
        }

        public async ValueTask DisposeAsync()
        {
            await baseStream.DisposeAsync();
        }
    }
}