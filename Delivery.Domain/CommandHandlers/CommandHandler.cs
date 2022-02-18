using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Domain.CommandHandlers
{
    public abstract class CommandHandler<TCommand, TResult>
    {
        protected IServiceProvider ServiceProvider { get; }
        
        protected  IExecutingRequestContextAdapter ExecutingRequestContext { get;  }
        
        public TCommand Command { get; set; }

        protected CommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            ServiceProvider = serviceProvider;
            ExecutingRequestContext = executingRequestContextAdapter;
        }

        public async Task<TResult> HandleCoreAsync(TCommand command)
        {
            TrackHandlerTrace($"{nameof(TCommand)}",  "initiated",ExecutingRequestContext.GetTelemetryProperties());
            var result = await HandleAsync(command);
            TrackHandlerTrace($"{nameof(TCommand)}",  "handled",ExecutingRequestContext.GetTelemetryProperties());
            return result;
        }
        
        public abstract Task<TResult> HandleAsync(TCommand command);

        private void TrackHandlerTrace(string command, string status, Dictionary<string, string> customerProperties)
        {
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{command} {status}", SeverityLevel.Information, customerProperties);
        }

    }
}