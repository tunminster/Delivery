using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Metrics
{
    public class ApplicationInsightsStopwatch : IAsyncDisposable
    {
        private readonly Dictionary<string, long> snapshots = new Dictionary<string, long>();
		private readonly Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();
		private readonly Stopwatch stopwatch = new Stopwatch();

		private readonly IServiceProvider serviceProvider;
		private readonly ApplicationInsightsStopwatchConfigurationDefinition stopwatchConfiguration;

		protected ApplicationInsightsStopwatch(IServiceProvider serviceProvider, ApplicationInsightsStopwatchConfigurationDefinition stopwatchConfiguration, Dictionary<string, string> telemetryProperties)
		{
			this.serviceProvider = serviceProvider;
			this.stopwatchConfiguration = stopwatchConfiguration;
			stopwatch.Start();
			this.telemetryProperties.AddRange(telemetryProperties);
		}

		/// <summary>
		///     Starts a new stopwatch that sends telemetry to Azure Application Insights
		/// </summary>
		public static ApplicationInsightsStopwatch Start(IServiceProvider serviceProvider, Dictionary<string, string>? telemetryProperties = null)
		{
			telemetryProperties ??= new Dictionary<string, string>();
			var applicationInsightsStopwatch = new ApplicationInsightsStopwatch(serviceProvider, new ApplicationInsightsStopwatchConfigurationDefinition(serviceProvider), telemetryProperties);
			return applicationInsightsStopwatch;
		}

		/// <summary>
		///     Starts a new stopwatch that sends telemetry to Azure Application Insights
		/// </summary>
		public static ApplicationInsightsStopwatch Start(IServiceProvider serviceProvider, ApplicationInsightsStopwatchConfigurationDefinition stopwatchConfiguration, Dictionary<string, string>? telemetryProperties = null)
		{
			telemetryProperties ??= new Dictionary<string, string>();
			var applicationInsightsStopwatch = new ApplicationInsightsStopwatch(serviceProvider, stopwatchConfiguration, telemetryProperties);
			return applicationInsightsStopwatch;
		}

		public ValueTask DisposeAsync()
		{
			Stop();
			return new ValueTask();
		}

		/// <summary>
		///     Starts taking a snapshot
		/// </summary>
		/// <param name="message">Message that describes the snapshot</param>
		public void StartSnapshot(string message)
		{
			EnsureStopwatchIsRunning();

			var elapsedTime = stopwatch.ElapsedMilliseconds;
			var snapshotKey = GetSnapshotKey(message);
			if (!snapshots.ContainsKey(snapshotKey))
			{
				snapshots[snapshotKey] = elapsedTime;
			}
		}

		public void Stop()
		{
			stopwatch.Stop();
		}

		/// <summary>
		///     Stops taking a snapshot
		/// </summary>
		/// <param name="message">Message that describes the snapshot</param>
		/// <param name="additionalTelemetryProperties">Optionally add additional telemetry properties</param>
		/// <returns>Duration that was measured for the snapshot, returns -1 if stopwatch is disabled</returns>
		public long StopSnapshot(string message, Dictionary<string, string>? additionalTelemetryProperties = null)
		{
			EnsureStopwatchIsRunning();

			if (additionalTelemetryProperties != null)
			{
				telemetryProperties.AddRange(additionalTelemetryProperties);
			}

			long snapshotDuration = -1;
			var elapsedTime = stopwatch.ElapsedMilliseconds;

			var snapshotKey = GetSnapshotKey(message);
			if (snapshots.ContainsKey(snapshotKey))
			{
				if (stopwatchConfiguration.IsEnabled)
				{
					snapshotDuration = elapsedTime - snapshots[snapshotKey];
					TrackMetric(message, snapshotDuration);
				}

				snapshots.Remove(snapshotKey);
			}

			return snapshotDuration;
		}

		/// <summary>
		///     Tracks a metric with the total time that was elapsed
		/// </summary>
		/// <param name="message">Message that describes the total elapsed time</param>
		/// <param name="additionalTelemetryProperties">Optionally add additional telemetry properties</param>
		public void TraceTotalElapsed(string message, Dictionary<string, string>? additionalTelemetryProperties = null)
		{
			if (stopwatchConfiguration.IsEnabled)
			{
				if (additionalTelemetryProperties != null)
				{
					telemetryProperties.AddRange(additionalTelemetryProperties);
				}

				var duration = stopwatch.ElapsedMilliseconds;
				TrackMetric(message, duration);
			}
		}

		private void EnsureStopwatchIsRunning()
		{
			if (!stopwatch.IsRunning)
			{
				throw new InvalidOperationException("Stopwatch is not running");
			}
		}

		private string GetSnapshotKey(string message)
		{
			var snapshotKey = message.Replace(" ", "-").ToLowerInvariant();
			return snapshotKey;
		}

		private void TrackMetric(string message, double duration)
		{
			Debug.WriteLine($"{message}: {duration}ms");
			serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric(message, duration, telemetryProperties);
		}
    }
}