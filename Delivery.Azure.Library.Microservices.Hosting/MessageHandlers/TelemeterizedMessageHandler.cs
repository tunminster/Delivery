using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Microservices.Hosting.Exceptions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Microservices.Hosting.MessageHandlers
{
    public class TelemeterizedMessageHandler
	{
		protected IServiceProvider ServiceProvider { get; }

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="serviceProvider">The kernel</param>
		/// <param name="executingRequestContextAdapter"></param>
		protected TelemeterizedMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
		{
			ServiceProvider = serviceProvider;
			ExecutingRequestContextAdapter = executingRequestContextAdapter;
		}

		/// <summary>
		///     Context properties used to provide more information in the telemetry
		/// </summary>
		public IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; }


		private IApplicationInsightsTelemetry Telemetry => ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

		/// <summary>
		///     Adds a new context property to the telemetry or updates an existing one
		/// </summary>
		/// <param name="propertyKey">Key of the property to add or update</param>
		/// <param name="propertyValue">Value to assign</param>
		protected void AddOrUpdateTelemetryContextProperty(string propertyKey, string propertyValue)
		{
			var telemetryProperties = ExecutingRequestContextAdapter.GetTelemetryProperties();
			telemetryProperties[propertyKey] = propertyValue;
		}

		/// <summary>
		///     Handles a failure during message processing
		/// </summary>
		/// <typeparam name="TState">Type that specifies the kind of processing</typeparam>
		/// <param name="currentProcessingState">Current state of the processing</param>
		/// <param name="exception">Exception that occured during processing</param>
		protected void HandleMessageProcessingFailure<TState>(TState currentProcessingState, Exception exception)
			where TState : Enum
		{
			throw new StatefulMessageProcessingException<TState>(currentProcessingState, ExecutingRequestContextAdapter.GetCorrelationId(), ExecutingRequestContextAdapter.GetTelemetryProperties(), exception);
		}

		/// <summary>
		///     Tracks an exception that occured
		/// </summary>
		/// <param name="exception">Exception that occured</param>
		/// <param name="additionalContextProperties">Additional contextual information</param>
		protected void TrackException(Exception exception, Dictionary<string, string> additionalContextProperties)
		{
			// Add current context properties to the additional ones given we don't want to pollute the telemetry for other telemetry by changing the current telemetry properties
			additionalContextProperties.AddRange(ExecutingRequestContextAdapter.GetTelemetryProperties());

			Telemetry.TrackException(exception, additionalContextProperties);
		}

		/// <summary>
		///     Writes a new business event
		/// </summary>
		/// <param name="eventName">Name of the event</param>
		protected void WriteEvent(string eventName)
		{
			Telemetry.TrackEvent(eventName);
		}

		/// <summary>
		///     Writes a new business event
		/// </summary>
		/// <param name="eventName">Name of the event</param>
		/// <param name="businessTelemetryContextProperties">Business-related context for the event</param>
		protected void WriteEvent(string eventName, Dictionary<string, string> businessTelemetryContextProperties)
		{
			// We do not add the telemetry context properties here as you will only want to have business oriented context
			Telemetry.TrackEvent(eventName, businessTelemetryContextProperties);
		}

		/// <summary>
		///     Writes a new value for a measurement and adds additional contextual information to the current context
		/// </summary>
		/// <param name="metricName">Name of the metric</param>
		/// <param name="measuredValue">Value that was measured</param>
		/// <param name="additionalContextProperties">Additional contextual information</param>
		protected void WriteMeasurement(string metricName, int measuredValue, Dictionary<string, string> additionalContextProperties)
		{
			Telemetry.TrackMetric(metricName, measuredValue, additionalContextProperties);
		}

		/// <summary>
		///     Writes a traces and adds additional contextual information to the current context
		/// </summary>
		/// <param name="text">Text to write</param>
		/// <param name="severityLevel">Severity level of the trace</param>
		protected void WriteTrace(string text, SeverityLevel severityLevel = SeverityLevel.Information)
		{
			WriteTrace(text, new Dictionary<string, string>(), severityLevel);
		}

		/// <summary>
		///     Writes a traces and adds additional contextual information to the current context
		/// </summary>
		/// <param name="text">Text to write</param>
		/// <param name="additionalContextProperties">Additional contextual information</param>
		/// <param name="severityLevel">Severity level of the trace</param>
		protected void WriteTrace(string text, Dictionary<string, string> additionalContextProperties, SeverityLevel severityLevel = SeverityLevel.Information)
		{
			// Add current context properties to the additional ones given we don't want to pollute the telemetry for other telemetry by changing the current telemetry properties
			additionalContextProperties.AddRange(ExecutingRequestContextAdapter.GetTelemetryProperties());

			Telemetry.TrackTrace(text, severityLevel, additionalContextProperties);
		}
	}
}