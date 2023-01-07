using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Microservices.Hosting.Exceptions
{
    [Serializable]
	public class StatefulMessageProcessingException<TState> : MessageProcessingException
	{
		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="currentState">Current state of the processing</param>
		/// <param name="correlationId">Correlation id used during the processing</param>
		/// <param name="telemetryContextProperties">
		///     Additional information for telemetry that provides more context concerning the
		///     processing
		/// </param>
		/// <param name="exception">Exception that occured during processing</param>
		/// <param name="message">Message that provides more information about the exception</param>
		public StatefulMessageProcessingException(TState currentState, string correlationId, Dictionary<string, string> telemetryContextProperties, Exception exception, string message)
			: base(correlationId, telemetryContextProperties, exception, message)
		{
			CurrentState = currentState;
		}

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="currentState">Current state of the processing</param>
		/// <param name="correlationId">Correlation id used during the processing</param>
		/// <param name="telemetryContextProperties">
		///     Additional information for telemetry that provides more context concerning the
		///     processing
		/// </param>
		/// <param name="exception">Exception that occured during processing</param>
		public StatefulMessageProcessingException(TState currentState, string correlationId, Dictionary<string, string> telemetryContextProperties, Exception exception)
			: this(currentState, correlationId, telemetryContextProperties, exception, exception.Message)
		{
		}

		protected StatefulMessageProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		///     Current state of the processing
		/// </summary>
		// ReSharper disable once RedundantDefaultMemberInitializer - strange compiler warning saying enum is null when it can't be
		public TState CurrentState { get; } = default!;
	}
}