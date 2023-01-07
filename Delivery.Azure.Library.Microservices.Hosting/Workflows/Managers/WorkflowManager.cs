using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Contracts.Contracts;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Exceptions;
using Delivery.Azure.Library.Microservices.Hosting.Exceptions;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Extensions;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;
using Delivery.Azure.Library.Telemetry.Extensions;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Services;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Managers
{
    public class WorkflowManager : IWorkflowManager
    {
        private readonly IServiceProvider serviceProvider;

        private IPersistenceProvider PersistenceProvider => serviceProvider.GetRequiredService<IPersistenceProvider>();
        private IWorkflowController WorkflowController => serviceProvider.GetRequiredService<IWorkflowController>();
        private readonly List<WorkflowInstance> workflowInstances;

        public WorkflowManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            var singletonMemoryProvider = serviceProvider.GetRequiredService<ISingletonMemoryProvider>();
            const string fieldName = "_instances";
            var type = typeof(MemoryPersistenceProvider);

            if (!(GetInstanceField(type, singletonMemoryProvider, fieldName) is List<WorkflowInstance>
                    instanceCollection))
            {
                throw new InvalidOperationException(
                    $"Expected to find a private field with name {fieldName} on type {type.Name}");
            }

            workflowInstances = instanceCollection;
        }

        public async Task DisposeWorkflowAsync(string workflowId)
        {
            var instance = await PersistenceProvider.GetWorkflowInstance(workflowId);

            var lockToken = false;

            try
            {
                Monitor.Enter(workflowInstances, ref lockToken);
                try
                {
                    workflowInstances.Remove(instance);
                }
                catch (IndexOutOfRangeException)
                {
                    // item has been removed.
                }
            }
            finally
            {
                if (lockToken)
                {
                    Monitor.Exit(workflowInstances);
                }
            }
        }

        public async Task ExecuteStatefulWorkflowAsync<TDefinition, TWorkflowContract>(ExecutingRequestContext executingRequest,
            TWorkflowContract workflowDataContract, int version = 1, int timeOutSeconds = 600) where TDefinition : IWorkflow<TWorkflowContract> where TWorkflowContract : class, IWorkflowDataContract, new()
        {
            WorkflowState workflowState;
            WorkflowInstance workflowInstance;

            ExceptionDispatchInfo? exceptionDispatchInfo = null;
            CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(timeOutSeconds));

            var workflowDefinition = typeof(TDefinition).FullName;
            var workflowId = await WorkflowController.StartWorkflow(workflowDefinition, version, workflowDataContract);

            try
            {
                await CompetingConsumerPollWorkflowToEndAsync(workflowId, workflowDefinition,
                    cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                workflowInstance = await PersistenceProvider.GetWorkflowInstance(workflowId);
                workflowState = workflowInstance.GetWorkflowState();

                await DisposeWorkflowAsync(workflowId);
            }

            // While polling the Workflow status on exception was raised.
            // This exception could be a TimeoutException or TaskCancelledException or OperationCanceledException
            if (exceptionDispatchInfo?.SourceException != null)
            {
                throw new StatefulMessageProcessingException<WorkflowState>(workflowState,
                    executingRequest.CorrelationId!, executingRequest.GetTelemetryProperties(),
                    exceptionDispatchInfo.SourceException);
            }

            if (workflowInstance.Status is WorkflowStatus.Terminated or WorkflowStatus.Suspended &&
                workflowInstance.Data is IWorkflowStateContract workflowStateContract)
            {
                var exception = workflowStateContract.Exception ?? new Exception(
                    $"Workflow has no exception details! Ensure that terminated workflows have the original exception and stacktrace. Data: {workflowStateContract.ConvertToJson()}");
                throw new StatefulMessageProcessingException<WorkflowState>(workflowState,
                    executingRequest.CorrelationId, executingRequest.GetTelemetryProperties(), exception);
            }
            
            // When using Sagas and compensating steps, the workflow status is Complete.
            // If an error is encountered  when executing a set of steps in a saga, the compensating steps will be executed and thus the workflow will have the complete status
            // When a sga is not use the Workflow Status is Terminated or Suspended
            // A check has to be done if the workflow has any exception.

            if (workflowInstance.Status is WorkflowStatus.Complete && workflowInstance.Data is ICompensatingWorkflowStep
                    and IWorkflowStateContract {Exception: { }} stateContract)
            {
                throw new StatefulMessageProcessingException<WorkflowState>(workflowState,
                    executingRequest.CorrelationId, executingRequest.GetTelemetryProperties(), stateContract.Exception);
            }
            
            
        }

        public async Task<SrWorkflowInstance<TWorkflowContract>> ExecuteWorkflowAsync<TDefinition, TWorkflowContract>(TWorkflowContract workflowDataContract, int version = 1,
            int timeOutSeconds = 600) 
            where TDefinition : IWorkflow<TWorkflowContract> 
            where TWorkflowContract : class, IWorkflowDataContract, new()
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeOutSeconds));
            var workflowDefinition = typeof(TDefinition).FullName!;
            var workflowId = await WorkflowController.StartWorkflow(workflowDefinition, version, workflowDataContract);
            await PollWorkflowToEndAsync(workflowId, workflowDefinition, cancellationTokenSource.Token);
            var workflowInstance = await PersistenceProvider.GetWorkflowInstance(workflowId);
            var workflowContract = await GetDataAsync<TWorkflowContract>(workflowId);
            var srWorkflowInstance = new SrWorkflowInstance<TWorkflowContract>
            {
                CompleteTime = workflowInstance.CompleteTime,
                CreateTime = workflowInstance.CreateTime,
                Data = workflowContract,
                Description = workflowInstance.Description,
                ExecutionPointers = workflowInstance.ExecutionPointers,
                Id = workflowInstance.Id,
                NextExecution = workflowInstance.NextExecution,
                Reference = workflowInstance.Reference,
                Status = workflowInstance.Status,
                Version = workflowInstance.Version,
                WorkflowDefinitionId = workflowInstance.WorkflowDefinitionId
            };
            await DisposeWorkflowAsync(workflowId);
            return srWorkflowInstance;
        }

        private async Task CompetingConsumerPollWorkflowToEndAsync(string workflowId, string workflowDefinition,
            CancellationToken cancellationToken)
        {
            var status = await GetStatusAsync(workflowId, cancellationToken);

            while (status == WorkflowStatus.Runnable && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(millisecondsDelay: 100, cancellationToken);
                status = await GetStatusAsync(workflowId, cancellationToken);
            }

            if (status == WorkflowStatus.Runnable)
            {
                throw new TimeoutException($"Workflow definition : {workflowDefinition} timed out");
            }
        }

        private async Task PollWorkflowToEndAsync(string workflowId, string workflowDefinition,
            CancellationToken cancellationToken)
        {
            var status = await GetStatusAsync(workflowId, cancellationToken);

            if (status == WorkflowStatus.Terminated)
            {
                throw new WorkflowServerException($"Workflow definition : {workflowDefinition} has thrown exception");
            }
            
            while (status == WorkflowStatus.Runnable && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(millisecondsDelay: 100, cancellationToken);
                status = await GetStatusAsync(workflowId, cancellationToken);
            }
            
            if (status == WorkflowStatus.Terminated)
            {
                throw new WorkflowServerException($"Workflow definition : {workflowDefinition} has thrown exception");
            }

            if (status == WorkflowStatus.Runnable)
            {
                throw new TimeoutException($"Workflow definition : {workflowDefinition} timed out");
            }
            
            
        }

        private static object? GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Static;

            var field = type.GetField(fieldName, bindFlags);

            if (field == null)
            {
                throw new InvalidOperationException(
                    $"Expected to find a private field with name {fieldName} on type {type.Name}");
            }

            return field.GetValue(instance);
        }

        private async Task<WorkflowStatus> GetStatusAsync(string workflowId, CancellationToken cancellationToken = default)
        {
            var instance = await PersistenceProvider.GetWorkflowInstance(workflowId, cancellationToken);
            return instance.Status;
        }

        private async Task<TWorkflowContract> GetDataAsync<TWorkflowContract>(string workflowId)
            where TWorkflowContract : IWorkflowDataContract, new()
        {
            var instance = await PersistenceProvider.GetWorkflowInstance(workflowId);

            switch (instance.Data)
            {
                case Exception exception:
                    ExceptionDispatchInfo.Capture(exception).Throw();
                    break;
                case TWorkflowContract workflowContract:
                    return workflowContract;
            }

            throw new NotSupportedException(
                $"Expected the workflow data contract to be an exception or {typeof(TWorkflowContract).Name}, instead found: {instance.Data?.GetType()}");
        }
    }
}