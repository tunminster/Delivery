using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts;
using WorkflowCore.Interface;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Managers
{
    public interface IWorkflowManager
    {
        Task DisposeWorkflowAsync(string workflowId);

        Task ExecuteStatefulWorkflowAsync<TDefinition, TWorkflowContract>(ExecutingRequestContext executingRequest,
            TWorkflowContract workflowDataContract, int version = 1, int timeOutSeconds = 600)
            where TDefinition : IWorkflow<TWorkflowContract>
            where TWorkflowContract : class, IWorkflowDataContract, new();

        Task<SrWorkflowInstance<TWorkflowContract>> ExecuteWorkflowAsync<TDefinition, TWorkflowContract>(
            TWorkflowContract workflowDataContract, int version = 1, int timeOutSeconds = 600)
            where TDefinition : IWorkflow<TWorkflowContract>
            where TWorkflowContract : class, IWorkflowDataContract, new();
    }
}