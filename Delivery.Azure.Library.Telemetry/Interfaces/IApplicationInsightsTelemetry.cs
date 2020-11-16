using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.Enums;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.Telemetry.Interfaces
{
    /// <summary>
    ///     Composes the <see cref="TelemetryClient" /> to provide simplified usage and access to application insights
    /// </summary>
    public interface IApplicationInsightsTelemetry
    {
        /// <summary>
        ///     Track a specific custom event
        /// </summary>
        /// <param name="eventName">Name of event</param>
        /// <param name="customProperties">Custom properties that provide additional context, if any</param>
        void TrackEvent(string eventName, Dictionary<string, string>? customProperties = null);

        /// <summary>
        ///     Tracks a specific dependency
        /// </summary>
        /// <param name="dependencyName">Name of the dependency</param>
        /// <param name="measuredDependencyType">Type of de dependency</param>
        /// <param name="duration">Duration of the interaction with the dependency</param>
        /// <param name="data">Additional information about the interaction</param>
        /// <param name="dependencyTarget">Target dependency</param>
        /// <param name="isSuccessful">Indication whether or not the interaction was successful or not</param>
        /// <param name="customProperties">List of custom properties to add</param>
        void TrackDependency(string dependencyName, MeasuredDependencyType measuredDependencyType, TimeSpan duration, string data, string dependencyTarget, bool? isSuccessful = null, Dictionary<string, string>? customProperties = null);

        /// <summary>
        ///     Track an unhandled exception
        /// </summary>
        /// <param name="exception">Unhandled exception that occured</param>
        /// <param name="customProperties">List of custom properties to add</param>
        void TrackException(Exception exception, Dictionary<string, string>? customProperties = null);

        /// <summary>
        ///     Track a specific metric
        /// </summary>
        /// <param name="metricName">Name of the metric</param>
        /// <param name="value">Sum of values from the metric sample</param>
        /// <param name="customProperties">Custom properties that provide additional context, if any</param>
        void TrackMetric(string metricName, double value, Dictionary<string, string>? customProperties = null);

        /// <summary>
        ///     Tracks a request
        /// </summary>
        /// <param name="response">Response</param>
        /// <param name="duration">Duration of the processing, if any</param>
        /// <param name="customProperties">Custom properties that provide additional context, if any</param>
        Task TrackRequestAsync(HttpResponse response, TimeSpan duration = default, Dictionary<string, string>? customProperties = null);

        /// <summary>
        ///     Tracks a request
        /// </summary>
        /// <param name="response">Response</param>
        /// <param name="duration">Duration of the processing, if any</param>
        /// <param name="customProperties">Custom properties that provide additional context, if any</param>
        Task TrackRequestAsync(HttpResponseMessage response, TimeSpan duration = default, Dictionary<string, string>? customProperties = null);

        /// <summary>
        ///     Trace a message
        /// </summary>
        /// <param name="message">Message to trace</param>
        /// <param name="severityLevel">Severity level of the trace</param>
        /// <param name="customProperties">List of custom properties</param>
        /// <param name="sequence">Sequence Id of the trace</param>
        void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Information, Dictionary<string, string>? customProperties = null, string? sequence = null);

        /// <summary>
        ///     Ensures that telemetry data that is buffered is forwarded to application insights
        /// </summary>
        void Flush();
    }
}