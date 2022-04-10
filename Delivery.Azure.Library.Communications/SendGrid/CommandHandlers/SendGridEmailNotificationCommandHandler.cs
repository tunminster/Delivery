using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.Builder;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.Enums;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Azure.Library.Communications.SendGrid.Providers;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Environments;
using Delivery.Azure.Library.Configuration.Environments.Enums;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Communications.SendGrid.CommandHandlers
{
    public sealed class SendGridEmailNotificationCommandHandler : EmailNotificationCommandHandler
    {
        public const string EmailAddressDoNotSendPrefix = "do-not-send";
        

        public SendGridEmailNotificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public override async Task HandleAsync(EmailNotificationCommand command)
        {
            
            var isEmailNotificationEnabled = ServiceProvider.GetRequiredService<IConfigurationProvider>()
                .GetSettingOrDefault("EmailNotification", true);

            var environmentProvider = new EnvironmentProvider(ServiceProvider);
            var environment = environmentProvider.GetCurrentEnvironment();

            if (environment == RuntimeEnvironment.Prd)
            {
                command.SendGridEmailCreationContract.Settings.IsSandBoxMode = true;
            }

            if (isEmailNotificationEnabled)
            {
                var startTime = DateTimeOffset.UtcNow;
                var clientProvider = new SendGridEmailNotificationClientProvider(ServiceProvider,
                    ExecutingRequestContextAdapter, command.SendGridEmailCreationContract);

                PopulateEmailNotificationBuilder(clientProvider, command.SendGridEmailCreationContract);

                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace(
                        $"{nameof(SendGridEmailNotificationCommandHandler)} handled {nameof(HandleAsync)} builder: {command} and completed in  {DateTimeOffset.UtcNow.Subtract(startTime).TotalSeconds}",
                        SeverityLevel.Information, ExecutingRequestContextAdapter.GetTelemetryProperties());

                await clientProvider.SendNotificationAsync();
            }
            else
            {
                ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(NotificationFeatures.EmailNotification)} is turned off",
                        SeverityLevel.Information, ExecutingRequestContextAdapter.GetTelemetryProperties());
            }

            await new SendGridAuditCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                .HandleAsync(new SendGridAuditCommand(command));
        }

        private static void PopulateEmailNotificationBuilder(SendGridEmailNotificationClientProvider clientProvider,
            SendGridEmailCreationContract sendGridEmailCreationContract)
        {
            new SendGridEmailNotificationBuilder(clientProvider.GetNotificationType())
                .WithSubject(sendGridEmailCreationContract.Subject)
                .WithFooter(sendGridEmailCreationContract.Footer)
                .WithReplyTo(sendGridEmailCreationContract.EmailReplyTo)
                .WithSetFrom(sendGridEmailCreationContract.EmailSender)
                .WithRecipients(sendGridEmailCreationContract.EmailRecipients)
                .WithBccRecipients(sendGridEmailCreationContract.EmailAddressBccRecipients)
                .WithCcRecipients(sendGridEmailCreationContract.EmailAddressCcRecipients)
                .WithEmailHeaders(sendGridEmailCreationContract.EmailHeaders)
                .WithTemplate(sendGridEmailCreationContract.Template)
                .WithSandboxMode(sendGridEmailCreationContract.Settings.IsSandBoxMode)
                .WithOpenTracking(sendGridEmailCreationContract.Tracking.IsTrackEmailOpeningEnabled)
                .Build();
        }
    }
}