namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces
{
    public interface IWorkflowAnonymousRequestStartContract<out TStart>
        where TStart : IWorkflowAnonymousExecutingRequest
    {
        public TStart Start { get; }
    }
}