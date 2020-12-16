using System;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Configuration.Configurations.Definitions
{
    /// <summary>
    ///  Indicates an object which purely provides configuration values
    /// Dependencies
    /// </summary>
    public class ConfigurationDefinition
    {
        protected IServiceProvider ServiceProvider { get; }
        
        protected ConfigurationDefinition(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        
        protected virtual IConfigurationProvider ConfigurationProvider => ServiceProvider.GetRequiredService<IConfigurationProvider>();
        
    }
}