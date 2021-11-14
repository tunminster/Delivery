using System;
using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Driver.Domain.Configurations
{
    public class DriverFileConfigurations : ConfigurationDefinition
    {
        public DriverFileConfigurations(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        
        public List<string> GetValidFileExtensions()
        {
            var validExtensions = ValidFileExtensions.Split(",").ToList();
            return validExtensions;
        }

        public string ValidFileExtensions => ConfigurationProvider.GetSettingOrDefault("Driver-Valid-FileExtensions", ".pdf,.jpeg,.jpg,.png,.gif,.docx");

        /// <summary>
        ///     Define 5MB maximum file size
        /// </summary>
        public long MaximumTotalFilesSize => ConfigurationProvider.GetSettingOrDefault("Driverr-Total-Maximum-Files-Size", 5 * 1024 * 1000);
    }
}