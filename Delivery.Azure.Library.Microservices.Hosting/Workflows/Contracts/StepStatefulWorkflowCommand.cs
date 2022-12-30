using Delivery.Azure.Library.Contracts.Contracts;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public interface IStepDataInput<TStepInput>
    {
        TStepInput Data { get; init; }
        
        WorkflowState WorkflowState { get; init; }
        
        ExecutingRequestContext ExecutingRequestContext { get; init; }
        
    }

    public record StepDataInput<TStepInput>(WorkflowState WorkflowState,
        ExecutingRequestContext ExecutingRequestContext, TStepInput Data) : IStepDataInput<TStepInput>;
}