using System;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Configuration.Exceptions
{
    [Serializable]
    public class SecretNotFoundException : Exception
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="secretName">Name of the secret that was not found</param>
        public SecretNotFoundException(string secretName) : base($"Secret '{secretName}' was not found")
        {
            SecretName = secretName;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="secretName">Name of the secret that was not found</param>
        /// <param name="innerException">Inner exception that is relevant to not finding the secret</param>
        public SecretNotFoundException(string secretName, Exception innerException) : base($"Secret '{secretName}' was not found", innerException)
        {
            SecretName = secretName;
        }

        protected SecretNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            SecretName = info.GetString(nameof(SecretName));
        }

        /// <summary>
        ///     Name of the secret that was not found
        /// </summary>
        [DataMember]
        public string? SecretName { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(SecretName), SecretName);

            base.GetObjectData(info, context);
        }
    }
}