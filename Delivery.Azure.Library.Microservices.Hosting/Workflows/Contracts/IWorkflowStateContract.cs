using System;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public interface IWorkflowStateContract
    {
        WorkflowState State { get; init; }
        
        Exception? Exception { get; set; }
    }
}