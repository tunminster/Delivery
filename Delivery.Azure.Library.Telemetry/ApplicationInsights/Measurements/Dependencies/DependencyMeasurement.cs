using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Interfaces;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies
{
    public class DependencyMeasurement : DependencyMeasurementBuilder, IDependencyMeasurement
	{
		public DependencyMeasurement(IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}

		/// <summary>
		///     Provides information about the dependency that is being tracked
		///     (More information -
		///     https://docs.microsoft.com/en-us/azure/application-insights/application-insights-data-model-dependency-telemetry)
		/// </summary>
		/// <param name="name">Name of the command initiated with this dependency call</param>
		/// <param name="type">Name of the dependency type. Examples are Http, SQL, Azure table</param>
		/// <param name="data">Command initiated by this dependency call</param>
		/// <param name="target">Target site of a dependency call. Examples are server name, host address.</param>
		public IDependencyMeasurement ForDependency(string name, MeasuredDependencyType type, string data, string target)
		{
			SetDependencyInformation(name, type, data, target);

			return this;
		}

		/// <summary>
		///     Provides information about the service which will be called and that is being tracked
		/// </summary>
		/// <param name="dependentServiceUri">Uri of the service which will be called</param>
		/// <param name="data">Command initiated by this dependency call</param>
		public IDependencyMeasurement ForHttpDependency(Uri dependentServiceUri, string data)
		{
			SetDependencyInformation(dependentServiceUri, data);

			return this;
		}

		public async Task<T> TrackAsync<T>(Func<Task<T>> doFunc)
		{
			var isSuccessful = false;
			var stopwatch = new Stopwatch();
			T response;

			try
			{
				stopwatch.Start();
				response = await doFunc();
				isSuccessful = true;
			}
			finally
			{
				stopwatch.Stop();
				Telemetry.TrackDependency(Name, Type, stopwatch.Elapsed, Data, Target, isSuccessful, ContextualInformation);
			}

			return response;
		}

		public async Task TrackAsync(Func<Task> doFunc)
		{
			var isSuccessful = false;
			var stopwatch = new Stopwatch();

			try
			{
				stopwatch.Start();
				await doFunc();
				isSuccessful = true;
			}
			finally
			{
				stopwatch.Stop();
				Telemetry.TrackDependency(Name, Type, stopwatch.Elapsed, Data, Target, isSuccessful, ContextualInformation);
			}
		}

		/// <summary>
		///     Specified contextual information that is related to this dependency measurement
		/// </summary>
		/// <param name="customTelemetryProperties">Contextual information</param>
		public IDependencyMeasurement WithContextualInformation(Dictionary<string, string> customTelemetryProperties)
		{
			SetContextualInformation(customTelemetryProperties);

			return this;
		}

		/// <summary>
		///     Specifies the correlation id to use
		/// </summary>
		/// <param name="correlationId">Correlation id that is used to link all telemetry</param>
		public IDependencyMeasurement WithCorrelationId(string correlationId)
		{
			SetCorrelationId(correlationId);

			return this;
		}
	}
}