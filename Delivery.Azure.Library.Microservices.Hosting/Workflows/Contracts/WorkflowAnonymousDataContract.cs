using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;
using FluentValidation.Results;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public record WorkflowAnonymousDataContract<TStart> : IWorkflowDataContract,
        IWorkflowAnonymousRequestStartContract<TStart>
        where TStart : IWorkflowAnonymousExecutingRequest
    {
        public ValidationResult ValidationResult { get; } = new();
        
#pragma warning disable CS8618
        public TStart Start { get; set; }
#pragma warning restore CS8618
    }
}