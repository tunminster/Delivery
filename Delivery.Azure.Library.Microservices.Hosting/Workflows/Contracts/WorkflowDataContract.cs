using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;
using FluentValidation.Results;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public record WorkflowDataContract<TStart> : IWorkflowDataContract, IWorkflowRequestStartContract<TStart>
        where TStart : IWorkflowExecutingRequest
    {
        public ValidationResult ValidationResult { get; } = new();
        
#pragma warning disable 8618
        public TStart Start { get; set; }
#pragma warning restore 8618
    }
}