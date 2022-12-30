using System;
using Delivery.Azure.Library.Contracts.Contracts;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows
{
    public abstract class StepContextBodyAsync<TInput> : StepAnonymousContextBodyAsync<TInput>
    {
        protected override AnonymousExecutingRequestContext AnonymousExecutingRequestContext => ExecutingRequestContext;
        
        protected abstract ExecutingRequestContext ExecutingRequestContext { get; }

        protected StepContextBodyAsync(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}