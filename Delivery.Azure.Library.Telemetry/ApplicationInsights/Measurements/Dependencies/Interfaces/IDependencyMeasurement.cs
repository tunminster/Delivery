using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Interfaces
{
    public interface IDependencyMeasurement
    {
        IDependencyMeasurement ForHttpDependency(Uri dependentServiceUri, string data);

        /// <summary>
        ///     Execute and track the action
        /// </summary>
        Task TrackAsync(Func<Task> doFunc);

        /// <summary>
        ///     Execute and track the action
        /// </summary>
        Task<T> TrackAsync<T>(Func<Task<T>> doFunc);

        IDependencyMeasurement WithContextualInformation(Dictionary<string, string> customTelemetryProperties);

        IDependencyMeasurement WithCorrelationId(string correlationId);
    }
}