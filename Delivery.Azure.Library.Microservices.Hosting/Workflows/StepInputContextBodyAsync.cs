using System;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows
{
    public abstract class StepInputContextBodyAsync<TInput, TStart> : StepContextBodyAsync<TInput>
        where TInput : IWorkflowRequestStartContract<TStart>
        where TStart : IWorkflowExecutingRequest
    {
        protected StepInputContextBodyAsync(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override AnonymousExecutingRequestContext AnonymousExecutingRequestContext =>
            Input.Start.ExecutingRequestContext;
    }
}