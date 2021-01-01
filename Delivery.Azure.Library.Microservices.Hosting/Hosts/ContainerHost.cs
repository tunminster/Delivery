using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Microservices.Hosting.Extensions;
using Delivery.Azure.Library.Microservices.Hosting.HostedServices;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace Delivery.Azure.Library.Microservices.Hosting.Hosts
{
    public abstract class ContainerHost
	{
		private readonly IHostBuilder hostBuilder;
		protected IServiceProvider ServiceProvider => Host!.Services;
		public IHost Host { get; private set; }

		protected IApplicationInsightsTelemetry ApplicationInsightsTelemetry => ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>();

		protected ContainerHost(IHostBuilder hostBuilder)
		{
			this.hostBuilder = hostBuilder;
		}

		/// <summary>
		///     Start running the application
		/// </summary>
		public async Task RunAsync(bool blockUntilCompletion = true)
		{
			try
			{
				Host = hostBuilder.UseConsoleLifetime().Build();
				var lifeCycleHost = Host.Services.GetServices<IHostedService>().OfType<LifetimeEventsHostedService?>().LastOrDefault();
				lifeCycleHost?.HostApplicationLifetime.ApplicationStopping.Register(() => new JoinableTaskContext().Factory.Run(async () => await OnStopAsync()));

				await OnStartAsync(blockUntilCompletion);
			}
			catch (Exception exception)
			{
				ApplicationInsightsTelemetry.TrackException(exception);
				throw;
			}
		}

		/// <summary>
		///     Provides capability to take actions when the host is ready
		/// </summary>
		protected virtual async Task OnStartAsync(bool blockUntilCompletion = true)
		{
			if (blockUntilCompletion)
			{
				Host!.Services.GetService<ILogger?>()?.Log(LogLevel.Information, "Host started up");
				await Host.RunAsync();
			}
			else
			{
				await Host!.StartAsync();
			}
		}

		/// <summary>
		///     Provides capability to take actions when the host is shutting down
		/// </summary>
		protected virtual async Task OnStopAsync()
		{
			await Task.CompletedTask;
		}

		private int? cachedRing;

		/// <summary>
		///     If set then the message processor only listens to messages for a given ring to ensure that processors don't pick up
		///     messages which they cannot handle yet
		/// </summary>
		public virtual int? Ring
		{
			get
			{
				if (cachedRing.HasValue)
				{
					return cachedRing;
				}

				return cachedRing = ServiceProvider.GetRuntimeRing();
			}
		}
	}
}