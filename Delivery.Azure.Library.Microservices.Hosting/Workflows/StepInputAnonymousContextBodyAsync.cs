using System;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows
{
    public abstract class StepInputAnonymousContextBodyAsync<TInput, TStart> : StepAnonymousContextBodyAsync<TInput>
        where TInput : IWorkflowAnonymousRequestStartContract<TStart>
        where TStart : IWorkflowAnonymousExecutingRequest
    {
        protected StepInputAnonymousContextBodyAsync(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override AnonymousExecutingRequestContext AnonymousExecutingRequestContext =>
            Input.Start.AnonymousExecutingRequestContext;
    }
}