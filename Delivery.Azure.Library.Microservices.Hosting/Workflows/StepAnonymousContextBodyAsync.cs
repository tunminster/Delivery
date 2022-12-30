using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Core.Extensions.Collections;
using Delivery.Azure.Library.Exceptions;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows
{
    public abstract class StepAnonymousContextBodyAsync<TInput> : StepBodyAsync
    {
        protected IServiceProvider ServiceProvider { get;  }
        protected ValidationResult ValidationResult { get; } = new();
        protected abstract AnonymousExecutingRequestContext AnonymousExecutingRequestContext { get;  }
        
        public TInput Input { get; set; }

#pragma warning disable CS8618
        protected StepAnonymousContextBodyAsync(IServiceProvider serviceProvider)
#pragma warning restore CS8618
        {
            ServiceProvider = serviceProvider;
        }

        protected void CopyValidations(ValidationResult validationResult)
        {
            ValidationResult.Errors.AddRange(validationResult.Errors);
        }

        protected void CopyValidations(IList<ValidationFailure> validationFailures)
        {
            ValidationResult.Errors.AddRange(validationFailures);
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            ExceptionDispatchInfo? exceptionDispatchInfo = null;
            
            TrackStepInTrace(context.Workflow.WorkflowDefinitionId, GetType().Name, AnonymousExecutingRequestContext.GetTelemetryProperties());

            try
            {
                await RunCoreAsync(context);
            }
            catch (ValidationException validationException)
            {
                ValidationResult.Errors.AddRange(validationException.Errors.Any()
                    ? validationException.Errors
                    : new List<ValidationFailure> {new(typeof(TInput).Name, validationException.Message)});
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException validationException)
            {
                ValidationResult.Errors.Add(new ValidationFailure(typeof(TInput).Name, validationException.Message));
            }
            catch (Exception exception)
            {
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(exception, AnonymousExecutingRequestContext.GetTelemetryProperties());
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(new WorkflowServerException($"Failed executing step {GetType().FullName}", exception));
            }
            TrackStepOutTrace(context.Workflow.WorkflowDefinitionId, GetType().Name, AnonymousExecutingRequestContext.GetTelemetryProperties());
            
            return Next(context, exceptionDispatchInfo);
        }

        protected abstract Task RunCoreAsync(IStepExecutionContext context);

        protected ExecutionResult Next(IStepExecutionContext context, ExceptionDispatchInfo? exceptionDispatchInfo)
        {
            if (ValidationResult.IsValid && exceptionDispatchInfo?.SourceException == null)
            {
                return ExecutionResult.Next();
            }

            // copy validations form the current step to the definition contracts
            context.ExecutionPointer.Status = PointerStatus.Failed;

            if (context.Workflow.Data is not ICompensatingWorkflowStep)
            {
                context.Workflow.Status =
                    !ValidationResult.IsValid ? WorkflowStatus.Suspended : WorkflowStatus.Terminated;
            }

            if (context.Workflow.Data is IWorkflowDataContract workflowData)
            {
                workflowData.ValidationResult.Errors.AddRange(ValidationResult.Errors);
            }

            if (context.Workflow.Data is IWorkflowStateContract workflowStateContract)
            {
                workflowStateContract.Exception = exceptionDispatchInfo?.SourceException;
            }

            var validationMessage = GetValidationMessage();
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Workflow {context.Workflow.Id} exited prematurely due to validations: {validationMessage}", SeverityLevel.Information, AnonymousExecutingRequestContext.GetTelemetryProperties());
            
            exceptionDispatchInfo?.Throw();
            return ExecutionResult.Next();
        }
        
        protected void TrackStepInTrace(string workflowDefinitionId, string stepName,
            Dictionary<string, string>? customProperties)
        {
            AddTraceValidations(customProperties);
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Workflow : {AttemptToGetWorkflowShortName(workflowDefinitionId) ?? workflowDefinitionId}-{stepName} - In", SeverityLevel.Information, customProperties);
        }
        
        protected void TrackStepOutTrace(string workflowDefinitionId, string stepName,
            Dictionary<string, string>? customProperties)
        {
            AddTraceValidations(customProperties);
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Workflow : {AttemptToGetWorkflowShortName(workflowDefinitionId) ?? workflowDefinitionId}-{stepName} - Out", SeverityLevel.Information, customProperties);
        }

        protected void TrackStepTrace(string workflowDefinitionId, string stepName, string message, SeverityLevel severityLevel,
            Dictionary<string, string>? customProperties)
        {
            AddTraceValidations(customProperties);
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Workflow : {AttemptToGetWorkflowShortName(workflowDefinitionId) ?? workflowDefinitionId}-{stepName} - Message {message}", severityLevel, customProperties);
        }

        protected void TrackStepMetric(string workflowDefinitionId, string stepName, string metricName, double value,
            Dictionary<string, string>? customProperties)
        {
            AddTraceValidations(customProperties);
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric($"Workflow : {AttemptToGetWorkflowShortName(workflowDefinitionId) ?? workflowDefinitionId}-{stepName}-{metricName}", value, customProperties);
        }

        protected void AddTraceValidations(Dictionary<string, string>? customProperties)
        {
            if (ValidationResult.Errors.Any())
            {
                customProperties?.Add("Validations", GetValidationMessage());
            }
        }

        protected string GetValidationMessage()
        {
            return string.Join(",", ValidationResult.Errors.Select(p => $"{p.Severity} - {p.ErrorMessage}"));
        }

        static string? AttemptToGetWorkflowShortName(string workflowDefinitionId)
        {
            var splitResult = workflowDefinitionId.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries);

            if (!splitResult.Any())
            {
                return null;
            }

            return splitResult[^1];
        }
        
    }
}