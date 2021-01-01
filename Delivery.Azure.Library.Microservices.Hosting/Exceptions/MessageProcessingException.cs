using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Microservices.Hosting.Exceptions
{
    [Serializable]
	public class MessageProcessingException : Exception
	{
		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="correlationId">Correlation id used during the processing</param>
		/// <param name="telemetryContextProperties">
		///     Additional information for telemetry that provides more context concerning the
		///     processing
		/// </param>
		/// <param name="exception">Exception that occured during processing</param>
		/// <param name="message">Message that provides more information about the exception</param>
		public MessageProcessingException(string correlationId, Dictionary<string, string> telemetryContextProperties, Exception exception, string message)
			: base(message, exception)
		{
			CorrelationId = correlationId;
			TelemetryContextProperties = new ReadOnlyDictionary<string, string>(telemetryContextProperties);
		}

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="correlationId">Correlation id used during the processing</param>
		/// <param name="telemetryContextProperties">
		///     Additional information for telemetry that provides more context concerning the
		///     processing
		/// </param>
		/// <param name="exception">Exception that occured during processing</param>
		public MessageProcessingException(string correlationId, Dictionary<string, string> telemetryContextProperties, Exception exception)
			: this(correlationId, telemetryContextProperties, exception, exception.Message)
		{
		}

		protected MessageProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		///     Correlation id used during the processing
		/// </summary>
		public string CorrelationId { get; } = string.Empty;

		/// <summary>
		///     Additional information for telemetry that provides more context concerning the processing
		/// </summary>
		public IReadOnlyDictionary<string, string> TelemetryContextProperties { get; } = new Dictionary<string, string>();
	}
}