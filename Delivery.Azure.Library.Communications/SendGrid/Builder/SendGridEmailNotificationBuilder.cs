using System.Collections.Generic;
using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts;
using SendGrid.Helpers.Mail;

namespace Delivery.Azure.Library.Communications.SendGrid.Builder
{
    public sealed class SendGridEmailNotificationBuilder
    {
        private readonly SendGridMessage sendGridMessage;

        public SendGridEmailNotificationBuilder(SendGridMessage sendGridMessage)
        {
            this.sendGridMessage = sendGridMessage;
        }

        public SendGridEmailNotificationBuilder WithSubject(string? subject)
        {
            sendGridMessage.Subject = subject;
            return this;
        }

        public SendGridEmailNotificationBuilder WithFooter(EmailFooterContract? emailFooterContract)
        {
            if (emailFooterContract != null)
            {
                sendGridMessage.SetFooterSetting(emailFooterContract.IsEnabled, emailFooterContract.Html, emailFooterContract.Text);
            }

            return this;
        }

        public SendGridEmailNotificationBuilder WithReplyTo(EmailContract emailContract)
        {
            sendGridMessage.SetReplyTo(new EmailAddress(emailContract.EmailAddress, emailContract.Name));
            return this;
        }

        public SendGridEmailNotificationBuilder WithSetFrom(EmailContract emailContract)
        {
            sendGridMessage.SetFrom(new EmailAddress(emailContract.EmailAddress, emailContract.Name));
            return this;
        }

        public SendGridEmailNotificationBuilder WithRecipients(List<EmailContract> emailContracts)
        {
            foreach (var emailContract in emailContracts)
            {
                sendGridMessage.AddTo(new EmailAddress(emailContract.EmailAddress, emailContract.Name));
            }

            return this;
        }

        public SendGridEmailNotificationBuilder WithBccRecipients(List<EmailContract> emailContracts)
        {
            foreach (var emailContract in emailContracts)
            {
                sendGridMessage.AddBcc(new EmailAddress(emailContract.EmailAddress, emailContract.Name));
            }

            return this;
        }
        
        public SendGridEmailNotificationBuilder WithCcRecipients(List<EmailContract> emailContracts)
        {
            foreach (var emailContract in emailContracts)
            {
                sendGridMessage.AddCc(new EmailAddress(emailContract.EmailAddress, emailContract.Name));
            }

            return this;
        }

        public SendGridEmailNotificationBuilder WithEmailHeaders(List<EmailHeaderContract> emailHeaderContracts)
        {
            foreach (var emailHeaderContract in emailHeaderContracts)
            {
                sendGridMessage.AddHeader(emailHeaderContract.Key, emailHeaderContract.Value);
            }

            return this;
        }

        public SendGridEmailNotificationBuilder WithTemplate(EmailTemplateContract emailTemplateContract)
        {
            sendGridMessage.SetTemplateId(emailTemplateContract.TemplateId);

            if (emailTemplateContract.DynamicTemplateData != null)
            {
                sendGridMessage.SetTemplateData(emailTemplateContract.DynamicTemplateData);
            }

            return this;
        }

        public SendGridEmailNotificationBuilder WithOpenTracking(bool isTrackEmailOpeningEnabled)
        {
            sendGridMessage.SetOpenTracking(isTrackEmailOpeningEnabled);
            return this;
        }

        public SendGridEmailNotificationBuilder WithSandboxMode(bool enable)
        {
            sendGridMessage.SetSandBoxMode(enable);
            return this;
        }

        public SendGridMessage Build()
        {
            return sendGridMessage;
        }
        
    }
}