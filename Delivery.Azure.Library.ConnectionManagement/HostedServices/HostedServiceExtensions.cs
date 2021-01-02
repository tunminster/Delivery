using System;
using Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Kernel;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.ConnectionManagement.HostedServices
{
    public static class HostedServiceExtensions
    {
        /// <summary>
        ///     Gets the hosted service managed by the <see cref="IMultipleTasksBackgroundService" />
        /// </summary>
        public static TRequired GetRequiredHostedService<TRequired>(this IServiceProvider serviceProvider)
        {
            var hostedService = serviceProvider.GetRequiredService<IHostedService, IMultipleTasksBackgroundService>();
            var service = hostedService.GetRequiredService<TRequired>();
            return service;
        }
    }
}