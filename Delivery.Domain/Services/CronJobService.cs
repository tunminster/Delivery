using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Domain.Services
{
    public abstract class CronJobService : IHostedService, IDisposable
    {
        private System.Timers.Timer timer;
        private readonly CronExpression expression;
        private readonly TimeZoneInfo timeZoneInfo;
        
        protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo)
        {
            expression = CronExpression.Parse(cronExpression);
            this.timeZoneInfo = timeZoneInfo;
        }  
        
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleJobAsync(cancellationToken);
        }
        
        protected virtual async Task ScheduleJobAsync(CancellationToken cancellationToken)
        {
            var next = expression.GetNextOccurrence(DateTimeOffset.Now, timeZoneInfo);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
                {
                    await ScheduleJobAsync(cancellationToken);
                }
                timer = new System.Timers.Timer(delay.TotalMilliseconds);
                
                #pragma warning disable VSTHRD101
                // ReSharper disable once AsyncVoidLambda
                timer.Elapsed += async (sender, args) =>
                {
                    timer.Dispose();  // reset and dispose timer
                    timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await DoWorkAsync(cancellationToken);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ScheduleJobAsync(cancellationToken);    // reschedule next
                    }
                };
#pragma warning restore VSTHRD101
                timer.Start();
            }
            await Task.CompletedTask;
        }
        
        public virtual async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);  // do the work
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Stop();
            await Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            timer?.Dispose();
        }
    }
    
    public interface IScheduleConfig<T>
    {
        string CronExpression { get; set; }
        TimeZoneInfo TimeZoneInfo { get; set; }
    }

    public class ScheduleConfig<T> : IScheduleConfig<T>
    {
        public string CronExpression { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
    }

    public static class ScheduledServiceExtensions
    {
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronJobService
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");
            }
            var config = new ScheduleConfig<T>();
            options.Invoke(config);
            if (string.IsNullOrWhiteSpace(config.CronExpression))
            {
                throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
            }

            services.AddSingleton<IScheduleConfig<T>>(config);
            services.AddHostedService<T>();
            return services;
        }
    }
}