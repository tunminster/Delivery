using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Http;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Exceptions
{
    public static class HttpExceptionWriter
    {
        /// <summary>
        ///     Formats the <see cref="HttpResponseMessage" /> to give a string with request/response content and headers
        /// </summary>
        public static async Task<string> FormatHttpResponseMessageAsync(this HttpResponseMessage httpResponseMessage)
        {
            var message = $"Status code {httpResponseMessage.StatusCode} ({Enum.GetName(typeof(HttpStatusCode), httpResponseMessage.StatusCode)}) from {httpResponseMessage.RequestMessage?.RequestUri?.AbsoluteUri ?? "Unknown Request"} {httpResponseMessage.RequestMessage?.Method.ToString() ?? "Unknown Http Method"}";
            var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(contentStream);
            var content = await streamReader.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                message += $"\r\nContent:\r\n{content}";
            }

            if (httpResponseMessage.RequestMessage != null)
            {
                message += $"\r\nRequest Headers:\r\n{httpResponseMessage.RequestMessage.Headers.Where(header => !HttpHeadersExtensions.SensitiveHeaders.Select(p => p.ToLowerInvariant()).Contains(header.Key.ToLowerInvariant())).Format()}";

                if (httpResponseMessage.RequestMessage.Content != null)
                {
                    var requestContentStream = await httpResponseMessage.RequestMessage.Content.ReadAsStreamAsync();
                    using var streamReaderResponse = new StreamReader(requestContentStream);
                    var responseContent = await streamReaderResponse.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        message += $"\r\nRequest Content:\r\n{responseContent}";
                    }
                }
            }

            message += $"\r\nResponse Headers:\r\n{httpResponseMessage.Headers.Format()}";

            return message;
        }
    }
}