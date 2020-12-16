using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry
{
    /// <summary>
    ///     Enables response writing since the response stream is not readable on its own
    /// </summary>
    public class WrappedResponseBodyStream : Stream
    {
        public WrappedResponseBodyStream(Stream stream)
        {
            Stream = stream;
            CopiedStream = new MemoryStream();
        }

        public Stream Stream { get; }

        public MemoryStream CopiedStream { get; }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await Stream.FlushAsync(cancellationToken);
            await CopiedStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
            CopiedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Stream.WriteAsync(buffer, offset, count, cancellationToken);
            await CopiedStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => Stream.Length;

        public override long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }
    }
}