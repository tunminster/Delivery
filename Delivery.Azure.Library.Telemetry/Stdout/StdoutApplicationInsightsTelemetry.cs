using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Delivery.Azure.Library.Telemetry.Stdout
{
    public class StdoutApplicationInsightsTelemetry : IApplicationInsightsTelemetry
	{
		private readonly IServiceProvider serviceProvider;
		private readonly string source;
		private StdoutLogger? cachedStdOutLogger;

		private StdoutLogger StdOutLogger => cachedStdOutLogger ??= new StdoutLogger(serviceProvider, source);

		public StdoutApplicationInsightsTelemetry(IServiceProvider serviceProvider, string source)
		{
			this.serviceProvider = serviceProvider;
			this.source = source;
		}

		public virtual void TrackEvent(string eventName, Dictionary<string, string>? customProperties = null)
		{
			StdOutLogger.Logger.LogInformation($"Event tracked: {eventName}. Properties: {customProperties.Format()}");
		}

		public virtual void TrackDependency(string dependencyName, MeasuredDependencyType measuredDependencyType, TimeSpan duration, string data, string dependencyTarget, bool? isSuccessful = null, Dictionary<string, string>? customProperties = null)
		{
		}

		public virtual void TrackException(Exception exception, Dictionary<string, string>? customProperties = null)
		{
			StdOutLogger.Logger.LogError($"Exception tracked: {exception.Format()}. Properties: {customProperties.Format()}");
		}

		public virtual void TrackMetric(string metricName, double value, Dictionary<string, string>? customProperties = null)
		{
			StdOutLogger.Logger.LogInformation($"Metric tracked: {metricName} ({value}). Properties: {customProperties.Format()}");
		}

		public async Task TrackRequestAsync(HttpResponse response, TimeSpan duration = default, Dictionary<string, string>? customProperties = null)
		{
			var maybeBody = await response.HttpContext.Request.ReadBodyAsync();
			string requestContent = maybeBody.IsPresent ? maybeBody.Value : "<Body too large to be logged>";

			StdOutLogger.Logger.LogInformation($"Request tracked: {requestContent} ({duration}). Properties: {customProperties.Format()}");
		}

		public async Task TrackRequestAsync(HttpResponseMessage response, TimeSpan duration = default, Dictionary<string, string>? customProperties = null)
		{
			if (response.RequestMessage == null)
			{
				return;
			}

			var requestContent = string.Empty;
			if (response.RequestMessage?.Content != null)
			{
				var readStream = await response.RequestMessage.Content.ReadAsStreamAsync();
				using var streamReader = new StreamReader(readStream);
				requestContent = await streamReader.ReadToEndAsync();
			}

			StdOutLogger.Logger.LogInformation($"Request tracked: {response.RequestMessage?.RequestUri?.AbsoluteUri} (content: {requestContent}, headers: {(response.RequestMessage?.Headers).Format()}) ({duration}). Properties: {customProperties.Format()}");
		}

		public void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Information, Dictionary<string, string>? customProperties = null, string? sequence = null)
		{
			var logMessage = $"Trace tracked: {message} ({severityLevel}). Properties: {customProperties.Format()}";
			switch (severityLevel)
			{
				case SeverityLevel.Verbose:
					StdOutLogger.Logger.LogDebug(logMessage);

					break;
				case SeverityLevel.Information:
					StdOutLogger.Logger.LogInformation(logMessage);

					break;
				case SeverityLevel.Warning:
					StdOutLogger.Logger.LogWarning(logMessage);
					break;
				case SeverityLevel.Error:
					StdOutLogger.Logger.LogError(logMessage);
					break;
				case SeverityLevel.Critical:
					StdOutLogger.Logger.LogCritical(logMessage);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(severityLevel), severityLevel, message: null);
			}
		}

		public void Flush()
		{
			// not required
		}
	}
}