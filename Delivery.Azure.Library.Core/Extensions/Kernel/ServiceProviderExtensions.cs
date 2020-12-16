using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Core.Extensions.Kernel
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        ///     Allows to retrieve a service by its registered type and then selecting its exact type
        /// </summary>
        public static TRequired GetRequiredService<TRegistered, TRequired>(this IServiceProvider serviceProvider)
        {
            var expected = serviceProvider.GetServices<TRegistered>().OfType<TRequired>().SingleOrDefault();
            if (expected == null)
            {
                throw new InvalidOperationException($"Expected type {nameof(TRegistered)} to have an implementation of {nameof(TRequired)}, but it wasn't found in the kernel");
            }

            return expected;
        }
    }
}