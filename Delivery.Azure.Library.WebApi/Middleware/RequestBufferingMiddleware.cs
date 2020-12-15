using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.WebApi.Middleware
{
    public class RequestBufferingMiddleware
    {
        private readonly RequestDelegate next;

        public RequestBufferingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                WrapResponseBody(context);
                context.Request.EnableBuffering(HttpTelemetryWriter.MemoryBufferLoggingLimitBytes);
                await next(context);
            }
            finally
            {
                var responseBodyStream = (WrappedResponseBodyStream) context.Response.Body;
                context.Response.Body = responseBodyStream.Stream; // reset back to the original stream
                await responseBodyStream.CopiedStream.DisposeAsync();
            }
        }
        
        private static void WrapResponseBody(HttpContext httpContext)
        {
            if (!(httpContext.Response.Body is WrappedResponseBodyStream))
            {
                // use MemoryStream so that the response can be read later
                // may lead to more memory usage in the application
                httpContext.Response.Body = new WrappedResponseBodyStream(httpContext.Response.Body);
            }
        }
        
        
    }
}