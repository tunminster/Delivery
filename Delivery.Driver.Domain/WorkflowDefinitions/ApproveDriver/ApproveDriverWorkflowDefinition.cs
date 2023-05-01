using System.Collections.Immutable;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Delivery.Driver.Domain.WorkflowDefinitions.ApproveDriver.Steps;
using WorkflowCore.Interface;

namespace Delivery.Driver.Domain.WorkflowDefinitions.ApproveDriver;

public record ApproveDriverWorkflowDataCommand(ExecutingRequestContext ExecutingRequestContext,
    DriverApprovalContract DriverApprovalContract) : IWorkflowExecutingRequest;

public record ApproveDriverWorkflowDataContract : MessageStatefulWorkflowContract<ApproveDriverWorkflowDataCommand>
{
    public ApproveDriverStepResult? ApproveDriverStepResult { get; init; }
    
    public SendApprovedEmailStepResult? SendApprovedEmailStepResult { get; init; }
}

public class ApproveDriverWorkflowDefinition : IWorkflow<ApproveDriverWorkflowDataContract>
{
    public void Build(IWorkflowBuilder<ApproveDriverWorkflowDataContract> builder)
    {
        builder
            .StartWith<ApproveDriverStep>()
            .Input(step => step.Input,
                data => new StepDataInput<ApproveDriverStepCommand>(data.State, data.Start.ExecutingRequestContext,
                    new ApproveDriverStepCommand(data.Start.DriverApprovalContract)))
            .Output(data => data.ApproveDriverStepResult, step => step.Output)
            .Then<SendApprovedEmailStep>()
            .Input(step => step.Input,
                data => new StepDataInput<SendApprovedEmailStepCommand>(data.State, data.Start.ExecutingRequestContext,
                    new SendApprovedEmailStepCommand(data.ApproveDriverStepResult!.DriverApprovalStatusContract)))
            .Output(data => data.SendApprovedEmailStepResult, step => step.Output);
    }

    public string Id => GetType().FullName!;

    public int Version => 1;
}