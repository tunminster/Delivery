using System;
using System.ComponentModel;
using Delivery.Azure.Library.Configuration.Configurations.Exceptions;
using Microsoft.Extensions.Configuration;
using IConfigurationProvider = Delivery.Azure.Library.Configuration.Configurations.Interfaces.IConfigurationProvider;

namespace Delivery.Azure.Library.Configuration.Configurations
{
    /// <summary>
    ///     Manages configuration settings by unifying common search locations
    ///     Dependencies:
    ///     [None]
    ///     Settings:
    ///     [None]
    /// </summary>
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfiguration configuration;

        public ConfigurationProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        ///     Gets the value for a specific application using the <see cref="IConfiguration" />
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="isMandatory">Indication if it is mandatory if this configuration is present</param>
        /// <returns>Configured value of the setting</returns>
        public string GetSetting(string name, bool isMandatory = true)
        {
            var settingValue = GetSetting<string>(name, isMandatory);
            return settingValue;
        }

        /// <summary>
        ///     Gets the value for a specific application using the <see cref="IConfiguration" />
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="isMandatory">Indication if it is mandatory if this configuration is present</param>
        /// <returns>Configured value of the setting</returns>
        /// <typeparam name="TSetting">Type of the expected setting value</typeparam>
        public TSetting GetSetting<TSetting>(string name, bool isMandatory = true)
        {
            var setting = configuration[name];

            if (string.IsNullOrEmpty(setting) && name.Contains("-"))
            {
				// retry with an underscore
				setting = configuration[name.Replace("-", "_")];
            }

            if (string.IsNullOrEmpty(setting) && name.Contains("_"))
            {
	            // retry with a hyphen
	            setting = configuration[name.Replace("_", "-")];
            }

            if (isMandatory && string.IsNullOrEmpty(setting))
            {
                throw new SettingNotFoundException(name);
            }

            return ConvertSetting<TSetting>(setting);
        }

        public string GetSettingOrDefault(string name, string defaultValue)
        {
            var settingValue = GetSettingOrDefault<string>(name, defaultValue);
            return settingValue;
        }

        public int GetSettingOrDefault(string name, int defaultValue)
        {
            var settingValue = GetSettingOrDefault<int>(name, defaultValue);
            return settingValue;
        }

        public bool GetSettingOrDefault(string name, bool defaultValue)
        {
            var settingValue = GetSettingOrDefault<bool>(name, defaultValue);
            return settingValue;
        }

        public double GetSettingOrDefault(string name, double defaultValue)
        {
            var settingValue = GetSettingOrDefault<double>(name, defaultValue);
            return settingValue;
        }

        public TSetting GetSettingOrDefault<TSetting>(string name, TSetting defaultValue)
        {
            var settingValue = GetSetting<string>(name, isMandatory: false);
            if (!string.IsNullOrWhiteSpace(settingValue))
            {
                var castValue = ConvertSetting<TSetting>(settingValue);
                return castValue;
            }

            return defaultValue;
        }

        private TSetting ConvertSetting<TSetting>(string settingValue)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(TSetting));
            if (!typeConverter.IsValid(settingValue))
            {
                throw new InvalidCastException($"Setting value '{settingValue}' cannot be cast to type {typeof(TSetting).Name}");
            }

            var castValue = ((TSetting) typeConverter.ConvertFromInvariantString(settingValue))!;
            return castValue;
        }
    }
}