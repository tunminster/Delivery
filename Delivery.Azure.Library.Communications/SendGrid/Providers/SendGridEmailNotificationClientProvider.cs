using System;
using System.IO;
using System.Threading.Tasks;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using Delivery.Azure.Library.Communications.SendGrid.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using SendGrid.Helpers.Errors.Model;
using SendGrid.Helpers.Mail;

namespace Delivery.Azure.Library.Communications.SendGrid.Providers
{
    public sealed class SendGridEmailNotificationClientProvider
    {
        private readonly SendGridMessage sendGridMessage;
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        private readonly SendGridEmailCreationContract sendGridEmailCreationContract;

        public SendGridEmailNotificationClientProvider(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter,
            SendGridEmailCreationContract sendGridEmailCreationContract)
        {
            sendGridMessage = new SendGridMessage();
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
            this.sendGridEmailCreationContract = sendGridEmailCreationContract;
        }

        public async Task<SendGridClient> GetNotificationClientAsync()
        {
            var sendGridConnection = await serviceProvider.GetRequiredService<ISendGridConnectionManager>()
                .GetConnectionAsync("Delivery", "SendGrid-Api-Key");

            return sendGridConnection.SendGridClient;
        }

        public SendGridMessage GetNotificationType()
        {
            return sendGridMessage;
        }

        public async Task SendNotificationAsync()
        {
            foreach (var emailAttachmentContract in sendGridEmailCreationContract.EmailAttachments)
            {
                await sendGridMessage.AddAttachmentAsync(emailAttachmentContract.FileName,
                    emailAttachmentContract.Stream, emailAttachmentContract.Type, emailAttachmentContract.Disposition,
                    emailAttachmentContract.ContentId);

                if (emailAttachmentContract.Stream != null)
                {
                    await emailAttachmentContract.Stream.DisposeAsync();
                }
            }

            var sendGridConnection = await GetNotificationClientAsync();

            var response = await sendGridConnection.SendEmailAsync(sendGridMessage);

            var bodyStream = await response.Body.ReadAsStreamAsync();
            using var streamReader = new StreamReader(bodyStream);
            var content = await streamReader.ReadToEndAsync();

            var message = $"SendGridClient response {content}-{response.StatusCode}";
            if (!response.IsSuccessStatusCode)
            {
                throw new SendGridInternalException(message);
            }

            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace(message,
                SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
        }
    }
}