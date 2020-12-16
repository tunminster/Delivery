using System;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Resiliency.Exceptions
{
    [Serializable]
    public class DependencyUnavailableException : Exception
    {
        public DependencyUnavailableException(string dependencyName, string details) : base(ComposeExceptionMessage(dependencyName))
        {
            Details = details;
            DependencyName = dependencyName;
        }

        public DependencyUnavailableException(string dependencyName, string details, Exception inner) : base(ComposeExceptionMessage(dependencyName), inner)
        {
            Details = details;
            DependencyName = dependencyName;
        }

        protected DependencyUnavailableException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            DependencyName = info.GetString(nameof(DependencyName));
            Details = info.GetString(nameof(Details));
        }

        /// <summary>
        ///     Name of the dependency that is unavailable
        /// </summary>
        [DataMember]
        public string? DependencyName { get; }

        /// <summary>
        ///     Details about the dependency failure
        /// </summary>
        [DataMember]
        public string? Details { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(DependencyName), DependencyName);
            info.AddValue(nameof(Details), Details);

            base.GetObjectData(info, context);
        }

        private static string ComposeExceptionMessage(string dependencyName)
        {
            return $"{dependencyName} is unavailable";
        }
    }
}