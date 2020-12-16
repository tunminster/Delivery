
using Delivery.Azure.Library.Configuration.Environments.Enums;

namespace Delivery.Azure.Library.Configuration.Environments.Interfaces
{
    /// <summary>
    ///     Unifies the concept of a known environment which an application is running in
    /// </summary>
    public interface IEnvironmentProvider
    {
        /// <summary>
        ///     Gets the current environment which an application is running in
        /// </summary>
        /// <exception cref="EnvironmentNotSupportedException">
        ///     Thrown when the expected environment setting doesn't exist in the known <see cref="RuntimeEnvironment" /> values
        /// </exception>
        RuntimeEnvironment GetCurrentEnvironment();

        /// <summary>
        ///     Gets the current ring which the runtime has been deployed into
        /// </summary>
        int? GetCurrentRing();

        /// <summary>
        ///     Returns <c>True</c> if the current environment matches the queried environment
        /// </summary>
        /// <exception cref="EnvironmentNotSupportedException">
        ///     Thrown when the expected environment setting doesn't exist in the known <see cref="RuntimeEnvironment" /> values
        /// </exception>
        bool IsEnvironment(RuntimeEnvironment runtimeEnvironment);
    }
}