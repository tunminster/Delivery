using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.ConnectionManagement.HostedServices.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Azure.Library.Microservices.Hosting.HostedServices
{
    public class LifetimeEventsHostedService : BackgroundService
	{
		private readonly IServiceProvider serviceProvider;
		public IHostApplicationLifetime HostApplicationLifetime { get; }

		public bool IsHostApplicationStarted { get; private set; }
		public bool IsHostApplicationStopping { get; private set; }
		public bool IsHostApplicationStopped { get; private set; }

		protected IApplicationInsightsTelemetry Telemetry => serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

		public LifetimeEventsHostedService(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime)
		{
			this.serviceProvider = serviceProvider;
			HostApplicationLifetime = hostApplicationLifetime;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Telemetry.TrackTrace($"Started running service {Assembly.GetEntryAssembly()?.GetName().Name}");

			HostApplicationLifetime.ApplicationStarted.Register(() => IsHostApplicationStarted = true);
			HostApplicationLifetime.ApplicationStopping.Register(ApplicationStopping());
			HostApplicationLifetime.ApplicationStopped.Register(() => IsHostApplicationStopped = true);

			await Task.CompletedTask;
		}

		private Action ApplicationStopping()
		{
			return () =>
			{
				Telemetry.TrackTrace($"Stopped running service {Assembly.GetEntryAssembly()?.GetName().Name}");
				Telemetry.Flush();

				WaitGracefulShutdown();

				IsHostApplicationStopping = true;
			};
		}

		private void WaitGracefulShutdown()
		{
			var gracefulShutdownTimeLimitMilliseconds = 120 * 1000;
			var multipleTasksBackgroundService = serviceProvider.GetServices<IHostedService>().OfType<IMultipleTasksBackgroundService>().SingleOrDefault();
			if (multipleTasksBackgroundService != null)
			{
				var timeWaitedForGracefulShutdownMilliseconds = 0;
				var backgroundProcessingServices = multipleTasksBackgroundService.GetHostedServices().OfType<IQueueWorkBackgroundService>();
				if (backgroundProcessingServices.Any())
				{
					foreach (var backgroundProcessingService in backgroundProcessingServices)
					{
						timeWaitedForGracefulShutdownMilliseconds = WaitForBackgroundProcessDrain(backgroundProcessingService, timeWaitedForGracefulShutdownMilliseconds, gracefulShutdownTimeLimitMilliseconds);
					}
				}
			}
		}

		private static int WaitForBackgroundProcessDrain(IQueueWorkBackgroundService backgroundProcessingService, int timeWaitedForGracefulShutdownMilliseconds, int gracefulShutdownTimeLimitMilliseconds)
		{
			while (backgroundProcessingService.TasksWaiting > 0)
			{
				// try to allow a graceful shutdown of work
				var millisecondsTimeout = 1 * 1000;
				Thread.Sleep(millisecondsTimeout);
				timeWaitedForGracefulShutdownMilliseconds += millisecondsTimeout;
				if (timeWaitedForGracefulShutdownMilliseconds > gracefulShutdownTimeLimitMilliseconds)
				{
					break;
				}
			}

			return timeWaitedForGracefulShutdownMilliseconds;
		}
	}
}