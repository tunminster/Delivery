using System;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public record MessageStatefulWorkflowContract<TStepDataCommand> : WorkflowDataContract<TStepDataCommand>, IWorkflowStateContract
        where TStepDataCommand : IWorkflowExecutingRequest
    {
        public WorkflowState State { get; init; } = new();
        
        public Exception? Exception { get; set; }
    }
}