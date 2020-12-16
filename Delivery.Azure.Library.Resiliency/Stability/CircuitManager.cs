using System;
using System.Collections.Concurrent;
using Delivery.Azure.Library.Resiliency.Stability.Configurations;
using Delivery.Azure.Library.Resiliency.Stability.Enums;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;

namespace Delivery.Azure.Library.Resiliency.Stability
{
    /// <summary>
	///     Manages <see cref="CircuitBreaker" /> connections
	///     Dependencies:
	///     <see cref="IConfigurationProvider" />
	///     <see cref="IApplicationInsightsTelemetry" />
	///     Settings:
	///     <see cref="CircuitManagerConfigurationDefinition" />
	/// </summary>
	public class CircuitManager : ICircuitManager
	{
		private readonly IServiceProvider serviceProvider;
		private readonly CircuitManagerConfigurationDefinition configurationDefinition;

		private readonly ConcurrentDictionary<string, ICircuitBreaker> circuitBreakers = new ConcurrentDictionary<string, ICircuitBreaker>();
		private readonly TimeSpan defaultDurationOfBreak = TimeSpan.FromSeconds(value: 2);

		public CircuitManager(IServiceProvider serviceProvider) : this(serviceProvider, new CircuitManagerConfigurationDefinition(serviceProvider))
		{
		}

		public CircuitManager(IServiceProvider serviceProvider, CircuitManagerConfigurationDefinition configurationDefinition)
		{
			this.serviceProvider = serviceProvider;
			this.configurationDefinition = configurationDefinition;
		}

		public ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency)
		{
			return GetCircuitBreaker(dependencyType, externalDependency, configurationDefinition.HandledEventsBeforeBreaking, defaultDurationOfBreak);
		}

		public ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency, TimeSpan durationOfBreak)
		{
			return GetCircuitBreaker(dependencyType, externalDependency, configurationDefinition.HandledEventsBeforeBreaking, durationOfBreak);
		}

		public ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency, string category)
		{
			return GetCircuitBreaker(dependencyType, externalDependency, category, configurationDefinition.HandledEventsBeforeBreaking, defaultDurationOfBreak);
		}

		private ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency, int handledEventsBeforeBreaking, TimeSpan durationOfBreak)
		{
			return GetCircuitBreaker(dependencyType, externalDependency, string.Empty, handledEventsBeforeBreaking, durationOfBreak);
		}

		public ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency, string category, int handledEventsBeforeBreaking, TimeSpan durationOfBreak)
		{
			var circuitKey = ComposeCircuitKey(externalDependency, category);

			if (circuitBreakers.ContainsKey(circuitKey))
			{
				return circuitBreakers[circuitKey];
			}

			var circuitBreaker = new CircuitBreaker(serviceProvider, dependencyType, externalDependency, category, handledEventsBeforeBreaking, durationOfBreak);

			circuitBreakers.AddOrUpdate(circuitKey, circuitBreaker, (_, _) => circuitBreaker);

			return circuitBreaker;
		}

		private static string ComposeCircuitKey(string externalDependency, string category)
		{
			category = category.Replace(" ", string.Empty);

			return string.IsNullOrWhiteSpace(category) ? $"{externalDependency}" : $"{externalDependency}-{category}";
		}
	}
}