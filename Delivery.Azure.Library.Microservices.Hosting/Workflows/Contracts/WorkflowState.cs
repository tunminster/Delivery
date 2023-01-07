using System;
using System.Collections.Generic;
using System.Linq;
using Delivery.Azure.Library.Core.Extensions.Json;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts
{
    public record WorkflowState
    {
        public static WorkflowState Empty => new();

        public Dictionary<string, StepState> StepStateCollection { get; init; } = new();

        public StepState? GetStepState<TStep>() where TStep : IStepBody
        {
            var stepKvp = StepStateCollection
                .FirstOrDefault(r => r.Key.Equals(typeof(TStep).Name, StringComparison.CurrentCultureIgnoreCase));

            if (stepKvp.Equals(default(KeyValuePair<string, StepState>)))
            {
                return default;
            }

            return stepKvp.Value;
        }
    }

    public record StepState
    {
        public PointerStatus? Status { get; init; }
        
        public string? OutputJson { get; init; }

        public TStepOutputResult? OutputAs<TStepOutputResult>()
        {
            if (string.IsNullOrWhiteSpace(OutputJson))
            {
                return default;
            }

            return OutputJson!.ConvertFromJson<TStepOutputResult>();
        }
    }
}