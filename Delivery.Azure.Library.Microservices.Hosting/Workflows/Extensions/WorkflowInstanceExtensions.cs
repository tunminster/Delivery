using System.Linq;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts;
using WorkflowCore.Models;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Extensions
{
    public static class WorkflowInstanceExtensions
    {
        public static WorkflowState GetWorkflowState(this WorkflowInstance workflowInstance)
        {
            if (workflowInstance.Data is not IWorkflowStateContract)
            {
                return WorkflowState.Empty;
            }

            var workflowState = new WorkflowState
            {
                StepStateCollection = workflowInstance
                    .ExecutionPointers
                    .Where(r => !string.IsNullOrWhiteSpace(r.StepName))
                    .ToDictionary(r => r.StepName,
                        r => new StepState
                        {
                            Status = r.Status,
                            OutputJson = r.ExtensionAttributes.ContainsKey(StepStatefulConstants.StepOutputJsonKey)
                                ? r.ExtensionAttributes[StepStatefulConstants.StepOutputJsonKey].ConvertToJson()
                                : null
                        })
            };

            return workflowState;
        }
    }
}