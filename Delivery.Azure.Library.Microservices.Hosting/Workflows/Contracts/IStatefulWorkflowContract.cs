using System;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public interface IStatefulWorkflowContract<TState> : IWorkflowDataContract
        where TState : Enum
    {
        TState State { get; set; }
    }
}