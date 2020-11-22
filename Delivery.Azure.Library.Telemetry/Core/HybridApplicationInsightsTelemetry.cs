using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace Delivery.Azure.Library.Telemetry.Core
{
    public class HybridApplicationInsightsTelemetry : IApplicationInsightsTelemetry
	{
		private readonly List<IApplicationInsightsTelemetry> telemetrySinks;

		internal HybridApplicationInsightsTelemetry(List<IApplicationInsightsTelemetry> telemetrySinks)
		{
			this.telemetrySinks = telemetrySinks;
		}

		public void TrackEvent(string eventName, Dictionary<string, string>? customProperties = null)
		{
			InteractWithSinks(sink => sink.TrackEvent(eventName, customProperties));
		}

		public void TrackDependency(string dependencyName, MeasuredDependencyType measuredDependencyType, TimeSpan duration, string data, string dependencyTarget, bool? isSuccessful = null, Dictionary<string, string>? customProperties = null)
		{
			InteractWithSinks(sink => sink.TrackDependency(dependencyName, measuredDependencyType, duration, data, dependencyTarget, isSuccessful, customProperties));
		}

		public void TrackException(Exception exception, Dictionary<string, string>? customProperties = null)
		{
			InteractWithSinks(sink => sink.TrackException(exception, customProperties));
		}

		public void TrackMetric(string metricName, double value, Dictionary<string, string>? customProperties = null)
		{
			InteractWithSinks(sink => sink.TrackMetric(metricName, value, customProperties));
		}

		public async Task TrackRequestAsync(HttpResponse response, TimeSpan duration = new TimeSpan(), Dictionary<string, string>? customProperties = null)
		{
			await InteractWithSinksAsync(sink => sink.TrackRequestAsync(response, duration, customProperties));
		}

		public async Task TrackRequestAsync(HttpResponseMessage response, TimeSpan duration = new TimeSpan(), Dictionary<string, string>? customProperties = null)
		{
			await InteractWithSinksAsync(sink => sink.TrackRequestAsync(response, duration, customProperties));
		}

		public void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Information, Dictionary<string, string>? customProperties = null, string? sequence = null)
		{
			InteractWithSinks(sink => sink.TrackTrace(message, severityLevel, customProperties));
		}

		public void Flush()
		{
			// not required
		}

		private void InteractWithSinks(Action<IApplicationInsightsTelemetry> writeAction)
		{
			foreach (var sink in telemetrySinks)
			{
				try
				{
					writeAction(sink);
				}
				catch
				{
					// Swallow
				}
			}
		}

		private async Task InteractWithSinksAsync(Func<IApplicationInsightsTelemetry, Task> writeFunc)
		{
			foreach (var sink in telemetrySinks)
			{
				try
				{
					await writeFunc(sink);
				}
				catch
				{
					// Swallow
				}
			}
		}

		public bool IsDisabled { get; } = false;
	}
}