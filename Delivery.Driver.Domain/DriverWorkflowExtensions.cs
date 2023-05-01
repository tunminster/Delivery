using System;
using Delivery.Azure.Library.Microservices.Hosting.Workflows.Extensions;
using Delivery.Driver.Domain.WorkflowDefinitions.ApproveDriver;
using Microsoft.AspNetCore.Builder;

namespace Delivery.Driver.Domain;

public static class DriverWorkflowExtensions
{
    /// <summary>
    ///  Register workflows used primary in synchronous (http) request processing
    /// </summary>
    /// <param name="applicationBuilder"></param>
    /// <returns></returns>
    public static IApplicationBuilder RegisterDriverWorkflows(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder
            .RegisterWorkflow<ApproveDriverWorkflowDefinition, ApproveDriverWorkflowDataContract>();

        return applicationBuilder;
    }

    /// <summary>
    ///  Register workflow used primary in background (message) processing scenarios
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IServiceProvider RegisterDriverWorkflows(this IServiceProvider serviceProvider)
    {
        serviceProvider
            .RegisterWorkflow<ApproveDriverWorkflowDefinition, ApproveDriverWorkflowDataContract>();

        return serviceProvider;
    }
    
}