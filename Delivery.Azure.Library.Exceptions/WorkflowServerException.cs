using System;
using System.Runtime.Serialization;

namespace Delivery.Azure.Library.Exceptions
{
    [Serializable]
    public class WorkflowServerException : Exception
    {
        public WorkflowServerException()
        {
        }

        public WorkflowServerException(string? message)
            : base(message)
        {
        }
        
        public WorkflowServerException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected WorkflowServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        
    }
}