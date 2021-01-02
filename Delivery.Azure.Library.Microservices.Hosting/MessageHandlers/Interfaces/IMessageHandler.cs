using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Delivery.Azure.Library.Microservices.Hosting.MessageHandlers.Interfaces
{
    public interface IMessageHandler : IAsyncDisposable, IDisposable
    {
        /// <summary>
        ///     Handle message
        /// </summary>
        Task HandleAsync();

        /// <summary>
        ///     Contextual information used in telemetry
        /// </summary>
        Dictionary<string, string> TelemetryContextProperties { get; }
    }
}