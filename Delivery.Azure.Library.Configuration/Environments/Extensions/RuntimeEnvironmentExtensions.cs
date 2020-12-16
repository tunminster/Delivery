using System;
using Delivery.Azure.Library.Configuration.Environments.Enums;

namespace Delivery.Azure.Library.Configuration.Environments.Extensions
{
    public static class RuntimeEnvironmentExtensions
    {
        public static string? GetEnvironmentName()
        {
            var aspnetcoreEnvironmentKey = "ASPNETCORE_ENVIRONMENT";
            var environmentName = Environment.GetEnvironmentVariable(aspnetcoreEnvironmentKey);

#if DEBUG
            if (string.IsNullOrEmpty(environmentName))
            {
                environmentName = RuntimeEnvironment.Dev.ToString();
            }
#endif
            return environmentName;
        }
    }
}