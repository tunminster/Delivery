using System;
using Delivery.Azure.Library.Resiliency.Stability.Enums;

namespace Delivery.Azure.Library.Resiliency.Stability.Interfaces
{
    /// <summary>
	///     Manages <see cref="ICircuitBreaker" /> connections
	/// </summary>
	public interface ICircuitManager
	{
		/// <summary>
		///     Gets a circuit breaker to prevent being impacted by an external dependency
		/// </summary>
		/// <param name="dependencyType">Indication on what type of dependency we are relying on</param>
		/// <param name="externalDependency">Indication on what dependency we rely on</param>
		ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency);

		/// <summary>
		///     Gets a circuit breaker to prevent being impacted by an external dependency
		/// </summary>
		/// <param name="dependencyType">Indication on what type of dependency we are relying on</param>
		/// <param name="externalDependency">Indication on what dependency we rely on</param>
		/// <param name="durationOfBreak">Duration of in which the circuit is open</param>
		ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency, TimeSpan durationOfBreak);

		/// <summary>
		///     Gets a circuit breaker to prevent being impacted by an external dependency
		/// </summary>
		/// <param name="dependencyType">Indication on what type of dependency we are relying on</param>
		/// <param name="externalDependency">Indication on what dependency we rely on</param>
		/// <param name="category">Specific category for that external dependency</param>
		ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency, string category);

		/// <summary>
		///     Gets a circuit breaker to prevent being impacted by an external dependency
		/// </summary>
		/// <param name="dependencyType">Indication on what type of dependency we are relying on</param>
		/// <param name="externalDependency">Indication on what dependency we rely on</param>
		/// <param name="category">Specific category for that external dependency</param>
		/// <param name="handledEventsBeforeBreaking">Number of handled events before breaking the circuit</param>
		/// <param name="durationOfBreak">Duration of in which the circuit is open</param>
		ICircuitBreaker GetCircuitBreaker(DependencyType dependencyType, string externalDependency, string category, int handledEventsBeforeBreaking, TimeSpan durationOfBreak);
	}
}