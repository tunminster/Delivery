using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces
{
    public interface IMultipleTasksBackgroundService : IHostedService
    {
        /// <summary>
        ///     Gets a list of all hosted services being run
        /// </summary>
        /// <returns></returns>
        List<IHostedService> GetHostedServices();

        /// <summary>
        ///     Gets a single hosted service
        /// </summary>
        TService GetRequiredService<TService>();
    }
}