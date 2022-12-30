using System;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public record StateWorkflowContract<TStart, TState> : WorkflowDataContract<TStart>, IStatefulWorkflowContract<TState>
        where TState : Enum
        where TStart : IWorkflowExecutingRequest
    {
#pragma warning disable CS8618
        public TState State { get; set; }
#pragma warning restore CS8618
    }
}