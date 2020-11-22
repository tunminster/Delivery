using System;
using System.Collections.Generic;

namespace Delivery.Azure.Library.Exceptions.Extensions
{
    public static class TelemetryExceptionExtensions
    {
        /// <summary>
        ///     Copies the custom properties to the exception so it can be logged properly
        /// </summary>
        public static Exception WithTelemetry(this Exception exception, IDictionary<string, string> customProperties)
        {
            foreach (var customProperty in customProperties)
            {
                if (!exception.Data.Contains(customProperty.Key))
                {
                    exception.Data.Add(customProperty.Key, customProperty.Value);
                }
            }

            return exception;
        }
    }
}