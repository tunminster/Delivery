using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Configuration.Environments.Enums;

namespace Delivery.Azure.Library.Configuration.Environments.Exceptions
{
    [Serializable]
    public class EnvironmentNotSupportedException : Exception
    {
        public EnvironmentNotSupportedException(string rawEnvironment) : base($"Current environment '{rawEnvironment}' is not known. Ensure that a setting exists with one of the following values: {string.Join(", ", Enum.GetNames(typeof(RuntimeEnvironment)))}")
        {
            Environment = rawEnvironment;
        }

        public EnvironmentNotSupportedException(string rawEnvironment, Exception innerException) : base($"Current environment '{rawEnvironment}' is not known. Ensure that a setting exists with one of the following values: {string.Join(", ", Enum.GetNames(typeof(RuntimeEnvironment)))}", innerException)
        {
            Environment = rawEnvironment;
        }

        protected EnvironmentNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Environment = info.GetString(nameof(Environment));
        }

        /// <summary>
        ///     Name of the environment that is not supported
        /// </summary>
        [DataMember]
        public string? Environment { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Environment), Environment);

            base.GetObjectData(info, context);
        }
    }
}