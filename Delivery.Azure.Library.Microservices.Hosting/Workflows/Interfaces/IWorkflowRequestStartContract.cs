namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces
{
    public interface IWorkflowRequestStartContract<out TStart>
        where TStart : IWorkflowExecutingRequest
    {
        public TStart Start { get; }
    }
}