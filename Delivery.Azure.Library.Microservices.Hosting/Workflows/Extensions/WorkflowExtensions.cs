using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Microservices.Hosting.Middlewares;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Contracts;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Interfaces;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Managers;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Delivery.Azure.Library.Microservices.Hosting.Workflows.Extensions
{
    public static class WorkflowExtensions
    {
        public static IApplicationBuilder
            RegisterWorkflow<TDefinition, TContract>(this IApplicationBuilder applicationBuilder)
            where TDefinition : IWorkflow<TContract> where TContract : new()
        {
            applicationBuilder.ApplicationServices.RegisterWorkflow<TDefinition, TContract>();
            return applicationBuilder;
        }

        public static IServiceProvider RegisterWorkflow<TDefinition, TContract>(this IServiceProvider serviceProvider)
            where TDefinition : IWorkflow<TContract> where TContract : new()
        {
            var host = serviceProvider.GetRequiredService<IWorkflowHost>();
            host.RegisterWorkflow<TDefinition, TContract>();
            return serviceProvider;
        }

        public static IApplicationBuilder StartWorkflowHost(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.ApplicationServices.StartWorkflowHost();
            return applicationBuilder;
        }

        public static void StartWorkflowHost(this IServiceProvider serviceCollection)
        {
            var host = serviceCollection.GetRequiredService<IWorkflowHost>();
            host.OnStepError += (workflow, _, exception) =>
            {
                serviceCollection.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(exception);

                if (workflow.Data is not ICompensatingWorkflowStep)
                {
                    workflow.Status = WorkflowStatus.Terminated;
                }

                if (workflow.Data is IWorkflowStateContract workflowStateContract)
                {
                    workflowStateContract.Exception = exception;
                }
                else
                {
                    workflow.Data = exception;
                }
            };
            host.Start();
        }

        public static void RegisterWorkflowSteps<TRootAssembly>(this IServiceCollection serviceCollection)
        {
            var entryAssembly = typeof(TRootAssembly).Assembly;
            if (entryAssembly == null)
            {
                throw new InvalidOperationException("Could not find the entry assembly");
            }

            foreach (var stepBodyAsyncType in entryAssembly.GetTypes().Where(p => p.IsSubclassOf(typeof(StepBodyAsync)) && !p.IsAbstract))
            {
                serviceCollection.AddTransient(stepBodyAsyncType);
            }

            var referencedAssemblies = entryAssembly
                .GetReferencedAssemblies()
                .Where(p => p.Name != null && p.Name.Contains("Delivery"))
                .ToList();

            foreach (var referencedAssembly in referencedAssemblies)
            {
                var assembly = Assembly.Load(referencedAssembly);
                var types = assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(StepBodyAsync)) && !p.IsAbstract);

                foreach (var stepBodyAsyncType in types)
                {
                    serviceCollection.AddTransient(stepBodyAsyncType);
                }
            }
            
        }

        public static void AddWorkflowCore<TRootAssembly>(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddWorkflow(wf =>
            {
                wf.EnableWorkflows = true;
                wf.EnablePolling = false;
                wf.EnableIndexes = false;
                wf.EnableEvents = false;
            });

            serviceCollection.AddTransient<IWorkflowMiddlewareErrorHandler, TelemetryWorkflowMiddlewareErrorHandler>();
            serviceCollection.AddSingleton<IWorkflowManager, WorkflowManager>();
            serviceCollection.RegisterWorkflowSteps<TRootAssembly>();
        }

        public static async Task StopWorkflowsAsync(this IServiceProvider serviceCollection,
            CancellationToken? cancellationToken)
        {
            var host = serviceCollection.GetRequiredService<IWorkflowHost>();
            await host.StopAsync(cancellationToken ?? CancellationToken.None);
        }

        public static async Task<TWorkflowContract> ExecuteWorkflowAsync<TDefinition, TWorkflowContract>(
            this IServiceProvider serviceProvider, TWorkflowContract workflowContract)
            where TDefinition : IWorkflow<TWorkflowContract>
            where TWorkflowContract : class, IWorkflowDataContract, new()
        {
            var workflow = serviceProvider.GetRequiredService<IWorkflowManager>();
            var workflowResult = await workflow.ExecuteWorkflowAsync<TDefinition, TWorkflowContract>(workflowContract);

            return workflowResult.Data;
        }
    }
}