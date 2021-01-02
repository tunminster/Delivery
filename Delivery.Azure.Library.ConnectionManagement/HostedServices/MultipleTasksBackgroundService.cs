using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.ConnectionManagement.HostedServices
{
    public class MultipleTasksBackgroundService : BackgroundService, IMultipleTasksBackgroundService
    {
        private readonly List<IHostedService> hostedServices = new();

        public MultipleTasksBackgroundService(params IHostedService[] hostedServices)
        {
            this.hostedServices.AddRange(hostedServices);
        }

        public List<IHostedService> GetHostedServices()
        {
            return hostedServices;
        }

        public TService GetRequiredService<TService>()
        {
            var target = hostedServices.OfType<TService>().SingleOrDefault();
            if (target == null)
            {
                throw new InvalidOperationException($"Expected to find a type of background service with type {typeof(TService).Name}, but none have been registered. Registered types: {string.Join(",", hostedServices.Select(p => p.GetType().Name))}");
            }

            return target;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>();
            hostedServices.ForEach(p => tasks.Add(p.StartAsync(stoppingToken)));
            await Task.WhenAll(tasks);
        }
    }
}