using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Domain.FrameWork.Messages
{
    public abstract class MessagePublisherAsync<TMessage> : IMessagePublisher
    {
        protected IServiceProvider ServiceProvider { get; }
        
        protected  IExecutingRequestContextAdapter ExecutingRequestContext { get;  }
        
        protected MessagePublisherAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            ServiceProvider = serviceProvider;
            ExecutingRequestContext = executingRequestContextAdapter;
        }

        public async Task ExecuteAsync(TMessage message)
        {
            TrackPublisherTrace($"{typeof(TMessage).Name}", "initiated",
                ExecutingRequestContext.GetTelemetryProperties());
            await PublishAsync(message);
            TrackPublisherTrace($"{typeof(TMessage).Name}", "published",
                ExecutingRequestContext.GetTelemetryProperties());
        }

        protected abstract Task PublishAsync(TMessage message);
        
        private void TrackPublisherTrace(string command, string status, Dictionary<string, string> customerProperties)
        {
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{command} {status}", SeverityLevel.Information, customerProperties);
        }

    }
}