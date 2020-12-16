namespace Delivery.Azure.Library.Configuration.Configurations.Interfaces
{
    /// <summary>
    ///     Manages configuration settings by unifying common search locations depending on the runtime of the application
    ///     The registered implementation should be appropriate for resolving configurations for the type of application
    /// </summary>
    /// <example>
    ///     For a web app the settings could use the.net core configuration settings, for a service fabric application
    ///     it could use the CodePackageActivationContext, for a console application it could use the supplied arguments etc
    /// </example>
    public interface IConfigurationProvider
    {
        /// <summary>
        ///     Gets a simple application setting value based on the key (name)
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="isMandatory">Indication if it is mandatory if this configuration is present</param>
        /// <returns>Configured value of the setting</returns>
        string GetSetting(string name, bool isMandatory = true);

        /// <summary>
        ///     Gets a simple application setting value based on the key (name)
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <param name="isMandatory">Indication if it is mandatory if this configuration is present</param>
        /// <returns>Configured value of the setting</returns>
        /// <typeparam name="TSetting">Type of the expected setting value</typeparam>
        TSetting GetSetting<TSetting>(string name, bool isMandatory = true);

        /// <summary>
        ///     Tries to get a setting with a given value, but if it doesn't exist then a default value is used as a fallback
        /// </summary>
        /// <param name="name">The setting name to find with the same logic as GetSetting</param>
        /// <param name="defaultValue">The default fallback value</param>
        string GetSettingOrDefault(string name, string defaultValue);

        /// <summary>
        ///     Tries to get a setting with a given value, but if it doesn't exist then a default value is used as a fallback
        /// </summary>
        /// <param name="name">The setting name to find with the same logic as GetSetting</param>
        /// <param name="defaultValue">The default fallback value</param>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the return type does not match what the setting is e.g. the setting
        ///     value is a double but an int is expected
        /// </exception>
        int GetSettingOrDefault(string name, int defaultValue);

        /// <summary>
        ///     Tries to get a setting with a given value, but if it doesn't exist then a default value is used as a fallback
        /// </summary>
        /// <param name="name">The setting name to find with the same logic as GetSetting</param>
        /// <param name="defaultValue">The default fallback value</param>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the return type does not match what the setting is e.g. the setting
        ///     value is a string but an bool is expected
        /// </exception>
        bool GetSettingOrDefault(string name, bool defaultValue);

        /// <summary>
        ///     Tries to get a setting with a given value, but if it doesn't exist then a default value is used as a fallback
        /// </summary>
        /// <param name="name">The setting name to find with the same logic as GetSetting</param>
        /// <param name="defaultValue">The default fallback value</param>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the return type does not match what the setting is e.g. the setting
        ///     value is a string but a double is expected
        /// </exception>
        double GetSettingOrDefault(string name, double defaultValue);

        /// <summary>
        ///     Tries to get a setting with a given value, but if it doesn't exist then a default value is used as a fallback
        /// </summary>
        /// <param name="name">The setting name to find with the same logic as GetSetting</param>
        /// <param name="defaultValue">The default fallback value</param>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the return type does not match what the setting is e.g. the setting
        ///     value is a string but a double is expected
        /// </exception>
        /// <typeparam name="TSetting">Type of the expected setting value</typeparam>
        TSetting GetSettingOrDefault<TSetting>(string name, TSetting defaultValue);
    }
}