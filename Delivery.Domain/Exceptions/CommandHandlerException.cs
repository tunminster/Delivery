using System;
using System.Runtime.Serialization;

namespace Delivery.Domain.Exceptions
{
    [Serializable]
    public class CommandHandlerException : Exception
    {
        public CommandHandlerException()
        {
        }

        public CommandHandlerException(string? message) : base(message)
        {
        }

        public CommandHandlerException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected CommandHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}