using WorkflowCore.Models;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    /// <summary>
    ///  This is very useful when a Step executes one or more Workflows.
    ///  This will help in capturing the state of a Workflow
    /// </summary>
    public class SrWorkflowInstance<TWorkflowContract> : WorkflowInstance where TWorkflowContract : IWorkflowDataContract
    {
        public new TWorkflowContract Data
        {
            get => (TWorkflowContract) base.Data;
            init => base.Data = value;
        }
    }
}