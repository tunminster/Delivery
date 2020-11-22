using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Core.Guards;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies
{
    public class DependencyMeasurementBuilder
    {
        private readonly IServiceProvider serviceProvider;
		public Dictionary<string, string> ContextualInformation { get; } = new Dictionary<string, string>();
		public string Data { get; private set; } = string.Empty;
		public string Name { get; private set; } = string.Empty;
		public string Target { get; private set; } = string.Empty;
		public MeasuredDependencyType Type { get; private set; }
		protected IApplicationInsightsTelemetry Telemetry => serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

		public DependencyMeasurementBuilder(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
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
		protected void SetDependencyInformation(string name, MeasuredDependencyType type, string data, string target)
		{
			Guard.Against(type == MeasuredDependencyType.None, nameof(type));

			Name = name;
			Type = type;
			Data = data;
			Target = target;
		}

		/// <summary>
		///     Provides information about the service which will be called and that is being tracked
		/// </summary>
		/// <param name="dependentServiceUri">Uri of the service which will be called</param>
		/// <param name="data">Command initiated by this dependency call</param>
		public void SetDependencyInformation(Uri dependentServiceUri, string data)
		{
			SetDependencyInformation(dependentServiceUri.AbsolutePath, MeasuredDependencyType.Http, data, dependentServiceUri.Host);
		}

		/// <summary>
		///     Specified contextual information that is related to this dependency measurement
		/// </summary>
		/// <param name="customTelemetryProperties">Contextual information</param>
		protected void SetContextualInformation(Dictionary<string, string> customTelemetryProperties)
		{
			ContextualInformation.AddRange(customTelemetryProperties);
		}

		/// <summary>
		///     Specifies the correlation id to use
		/// </summary>
		/// <param name="correlationId">Correlation id that is used to link all telemetry</param>
		protected void SetCorrelationId(string correlationId)
		{
			ContextualInformation[CustomProperties.CorrelationId] = correlationId;
		}

		/// <summary>
		///     Specifies the dependency data
		/// </summary>
		/// <param name="dependencyData">Data about the dependency</param>
		protected void SetDependencyData(string dependencyData)
		{
			Data = dependencyData;
		}
    }
}