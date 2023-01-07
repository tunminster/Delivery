
using FluentValidation.Results;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public interface IWorkflowDataContract
    {
        ValidationResult ValidationResult { get; }
    }
}