using System;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Configuration.Configurations.Exceptions
{
    [Serializable]
    public class SettingNotFoundException : Exception
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="settingName">Name of the setting that was not found</param>
        public SettingNotFoundException(string settingName) : base($"Setting '{settingName}' was not found")
        {
            SettingName = settingName;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="settingName">Name of the setting that was not found</param>
        /// <param name="innerException">Inner exception that is relevant to not finding the setting</param>
        public SettingNotFoundException(string settingName, Exception innerException) : base($"Setting '{settingName}' was not found", innerException)
        {
            SettingName = settingName;
        }

        protected SettingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SettingName = info.GetString(nameof(SettingName));
        }

        /// <summary>
        ///     Name of the settings that was not found
        /// </summary>
        [DataMember]
        public string? SettingName { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(SettingName), SettingName);

            base.GetObjectData(info, context);
        }
    }
}