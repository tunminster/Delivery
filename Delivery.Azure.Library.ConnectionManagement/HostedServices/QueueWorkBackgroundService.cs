using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.ConnectionManagement.HostedServices
{
    public class QueueWorkBackgroundService : BackgroundService, IQueueWorkBackgroundService
    {
        protected IServiceProvider ServiceProvider { get;  }
        
        private readonly Channel<Func<CancellationToken, Task>> channel = Channel.CreateUnbounded<Func<CancellationToken, Task>>();
        
        public int TasksWaiting { get; private set; }
        
        protected QueueWorkBackgroundService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = channel.Reader;
            while (await reader.WaitToReadAsync(stoppingToken))
            {
                if (reader.TryRead(out var item))
                {
#pragma warning disable 4014
                    // fire-and-forget task
                    Task.Run(async () =>
                    {
                        await item(stoppingToken);
                        TasksWaiting--;
                    }, stoppingToken);
#pragma warning restore 4014
                }
            }
        }

        public void EnqueueBackgroundWork(Func<CancellationToken, Task> workItem)
        {
            TasksWaiting++;
            channel.Writer.WriteAsync(workItem);
        }

    }
}