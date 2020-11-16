using System.Threading.Tasks;

namespace Delivery.Azure.Library.Configuration.Features.Interfaces
{
    public interface IFeatureProvider
    {
        /// <summary>
        ///     Determines if a feature toggle is active.
        ///     Features are active by default but should be disabled for environments where they are not ready
        /// </summary>
        /// <param name="featureFlagKey">Feature flag to verify</param>
        /// <param name="isEnabledByDefault">Indication whether or not the feature should be enabled by default or not</param>
        /// <example>key=FeatureFlag_VerboseCircuitBreakerTelemetry, value=False</example>
        Task<bool> IsEnabledAsync(string featureFlagKey, bool isEnabledByDefault = true);
    }
}