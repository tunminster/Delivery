using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Delivery.Azure.Library.Telemetry.Stdout
{
    public class StdoutLogger
    {
        private readonly IServiceProvider serviceProvider;

        private ILoggerFactory LoggerFactory => serviceProvider.GetService<ILoggerFactory?>() ?? Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());

        public ILogger Logger { get; }

        public StdoutLogger(IServiceProvider serviceProvider, string? source)
        {
            this.serviceProvider = serviceProvider;
            Logger = LoggerFactory.CreateLogger(source ?? "Unknown");
        }
    }
}