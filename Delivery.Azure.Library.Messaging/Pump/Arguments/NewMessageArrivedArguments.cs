using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Azure.ServiceBus;

namespace Delivery.Azure.Library.Messaging.Pump.Arguments
{
    public class NewMessageArrivedArguments
    {
        public NewMessageArrivedArguments(Message message, string correlationId, string cycleId, Dictionary<string, string> telemetryContextProperties)
        {
            Message = message;
            CorrelationId = correlationId;
            CycleId = cycleId;
            TelemetryContextProperties = new ReadOnlyDictionary<string, string>(telemetryContextProperties);
        }

        /// <summary>
        ///     Correlation id that assigned to the message
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        ///     Cycle id that assigned to the processing
        /// </summary>
        public string CycleId { get; }

        /// <summary>
        ///     New message that has arrived
        /// </summary>
        public Message Message { get; }

        /// <summary>
        ///     Additional information for telemetry that provides more context concerning the processing
        /// </summary>
        public IReadOnlyDictionary<string, string> TelemetryContextProperties { get; }
    }
}