using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Azure.Library.Communications.SendGrid.Services.EmailService;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Microservices.Hosting.Workflows;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Enums;
using Delivery.Driver.Domain.Contracts.V1.Models;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval;
using Delivery.Driver.Domain.Factories;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;

namespace Delivery.Driver.Domain.WorkflowDefinitions.ApproveDriver.Steps;

public record SendApprovedEmailStepCommand(DriverApprovalStatusContract DriverApprovalStatusContract);

public record SendApprovedEmailStepResult(DriverApprovalStatusContract DriverApprovalStatusContract);

public class SendApprovedEmailStep : StepStatefulContextBodyAsync<SendApprovedEmailStepCommand, SendApprovedEmailStepResult>
{
    public SendApprovedEmailStep(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<SendApprovedEmailStepResult> ExecuteStepAsync(IStepExecutionContext context)
    {
        var configurationProvider = ServiceProvider.GetRequiredService<IConfigurationProvider>();
        var notificationUniqueId = $"{Guid.NewGuid()}-driver-approved-notification";
        var emailTemplateId = configurationProvider.GetSetting("SendGrid-Driver-Approved-EmailTemplateId");
        var emailTo = Input.Data.DriverApprovalStatusContract.EmailAddress;
        var emailBcc = configurationProvider.GetSetting("SendGrid-On-Boarding-Bcc");
        
        var subject = "Thank you for joining us!";
        
        var dynamicTemplateData = new DriverApprovedDataModel
        {
            Name = Input.Data.DriverApprovalStatusContract.DriverName,
            Subject = subject,
        };
        
        var driverEmailTemplateModel = new DriverEmailTemplateModel(BusinessDomainType.Driver, emailTemplateId,
            subject, dynamicTemplateData, notificationUniqueId);

        var sendGridEmailCreationContract =
            new DriverEmailTemplateFactory(ServiceProvider).Create(driverEmailTemplateModel);
        
        sendGridEmailCreationContract.EmailRecipients.Add(new EmailContract { EmailAddress = emailTo});
        sendGridEmailCreationContract.EmailAddressBccRecipients.Add(new EmailContract {EmailAddress = emailBcc});

        await new SendGridEmailService(ServiceProvider, new ExecutingRequestContextAdapter(ExecutingRequestContext)).SendEmailAsync(
            sendGridEmailCreationContract);

        return new SendApprovedEmailStepResult(Input.Data.DriverApprovalStatusContract);
    }
}