using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Core.Exceptions
{
    /// <summary>
    ///     Indicates that a validation exception was raised for serialization reasons
    /// </summary>
    [Serializable]
    public class ValidationSerializationException : ValidationException
    {
        public ValidationSerializationException(ValidationResult validationResult, ValidationAttribute validatingAttribute, object value)
            : base(validationResult, validatingAttribute, value)
        {
        }

        public ValidationSerializationException(string errorMessage, ValidationAttribute validatingAttribute, object value)
            : base(errorMessage, validatingAttribute, value)
        {
        }

        public ValidationSerializationException()
        {
        }

        public ValidationSerializationException(string message) : base(message)
        {
        }

        public ValidationSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValidationSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}