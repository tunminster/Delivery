using System;
using Delivery.Azure.Library.Configuration.Environments.Configurations;
using Delivery.Azure.Library.Configuration.Environments.Enums;
using Delivery.Azure.Library.Configuration.Environments.Exceptions;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Core.Guards;

namespace Delivery.Azure.Library.Configuration.Environments
{
    /// <summary>
    ///     Unifies the concept of a known environment which an application is running in
    ///     Dependencies:
    ///     <see cref="IConfigurationProvider" />
    ///     Settings:
    ///     <see cref="EnvironmentConfigurationDefinition" />
    /// </summary>
    public class EnvironmentProvider : IEnvironmentProvider
    {
        private readonly EnvironmentConfigurationDefinition environmentConfigurationDefinition;

        public EnvironmentProvider(IServiceProvider serviceProvider) : this(new EnvironmentConfigurationDefinition(serviceProvider))
        {
        }

        public EnvironmentProvider(EnvironmentConfigurationDefinition environmentConfigurationDefinition)
        {
            this.environmentConfigurationDefinition = environmentConfigurationDefinition;
        }

        public RuntimeEnvironment GetCurrentEnvironment()
        {
            var rawCurrentEnvironment = Convert.ToString(environmentConfigurationDefinition.Environment).ToLowerInvariant();

            try
            {
                return (RuntimeEnvironment) Enum.Parse(typeof(RuntimeEnvironment), rawCurrentEnvironment, ignoreCase: true);
            }
            catch (Exception exception)
            {
                throw new EnvironmentNotSupportedException(rawCurrentEnvironment, exception);
            }
        }

        public int? GetCurrentRing()
        {
            return environmentConfigurationDefinition.Ring;
        }

        public bool IsEnvironment(RuntimeEnvironment runtimeEnvironment)
        {
            Guard.Against(runtimeEnvironment == RuntimeEnvironment.None, nameof(runtimeEnvironment));

            var currentEnvironment = GetCurrentEnvironment();
            return currentEnvironment == runtimeEnvironment;
        }
    }
}