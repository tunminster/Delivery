using System;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Core.Monads
{
    /// <summary>
    ///     Creates a wrapper around objects which forces the caller to handle the case when a value isn't there (null)
    /// </summary>
    [DataContract]
    public class Maybe<T>
    {
        [DataMember] private readonly T value;

        public Maybe(T value)
        {
            this.value = value;

            // If we have a value type, it's always present.
            if (typeof(T).IsValueType)
            {
                IsPresent = value != null;
            }
            else // If it's a reference type, it's present if the value is not null
            {
                IsPresent = this.value != null;
            }
        }

        private Maybe(T value, bool isPresent)
        {
            this.value = value;
            IsPresent = isPresent;
        }

        /// <summary>
        ///     Creates an instance for scenarios where there is no value
        /// </summary>
#pragma warning disable CS8601 // Possible null reference assignment
#pragma warning disable CS8604
#pragma warning disable CS8653

        public static Maybe<T> NotPresent { get; } = new Maybe<T>(value: default, isPresent: false);

        /// <summary>
        ///     The found value, if applicable
        /// </summary>
        public T Value
        {
            get
            {
                if (value == null || typeof(T).IsValueType && !IsPresent)
                {
                    throw new InvalidOperationException("No value is present");
                }

                return value;
            }
        }

        /// <summary>
        ///     Indication whether or not there is a value present
        /// </summary>
        [DataMember]
        public bool IsPresent { get; private set; }
#pragma warning restore CS8653
#pragma warning restore CS8604
#pragma warning disable CS8601 // Possible null reference assignment
    }
}