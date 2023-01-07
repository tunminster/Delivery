using System;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows
{
    public abstract class StepOutputAnonymousContextBodyAsync<TInput, TStart, TOutput> : StepInputAnonymousContextBodyAsync<TInput, TStart>
        where TInput : IWorkflowAnonymousRequestStartContract<TStart>
        where TStart : IWorkflowAnonymousExecutingRequest
    {
#pragma warning disable CS8618
        protected StepOutputAnonymousContextBodyAsync(IServiceProvider serviceProvider) : base(serviceProvider)
#pragma warning restore CS8618
        {
        }
        
        public TOutput Output { get; protected set; }
    }
}