using Delivery.Azure.Library.Contracts.Contracts;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces
{
    public interface IWorkflowAnonymousExecutingRequest
    {
        AnonymousExecutingRequestContext AnonymousExecutingRequestContext { get;  }
    }
}