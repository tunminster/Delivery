using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts;
using WorkflowCore.Interface;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows
{
    public abstract class StepStatefulContextBodyAsync<TInput, TOutput> : StepContextBodyAsync<IStepDataInput<TInput>>
    {
        public TOutput? Output { get; private set; }
        
        protected StepStatefulContextBodyAsync(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ExecutingRequestContext ExecutingRequestContext => Input.ExecutingRequestContext;

        protected override async Task RunCoreAsync(IStepExecutionContext context)
        {
            var executedStep = Input
                .WorkflowState
                .StepStateCollection
                .FirstOrDefault(x => x.Key.Equals(GetType().Name, StringComparison.InvariantCultureIgnoreCase))
                .Value;

            if (executedStep?.Status == WorkflowCore.Models.PointerStatus.Complete)
            {
                Output = executedStep.OutputJson!.ConvertFromJson<TOutput>();
                context.ExecutionPointer.ExtensionAttributes.Add(StepStatefulConstants.StepOutputJsonKey, Output);

                return;
            }

            Output = await ExecuteStepAsync(context);
            context.ExecutionPointer.ExtensionAttributes.Add(StepStatefulConstants.StepOutputJsonKey, Output);
        }

        protected abstract Task<TOutput> ExecuteStepAsync(IStepExecutionContext context);
    }
}