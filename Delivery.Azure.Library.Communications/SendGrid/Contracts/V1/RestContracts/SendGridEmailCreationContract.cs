using System.Collections.Generic;

namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record SendGridEmailCreationContract : EmailCreationContract
    {
        /// <summary>
        ///  Add category for this email notification
        /// </summary>
        public List<string> Categories { get; init; } = new();

        /// <summary>
        ///  A list of bcc recipient email addresses
        /// </summary>
        public List<EmailContract> EmailAddressBccRecipients { get; init; } = new();
        
        /// <summary>
        ///  A list of cc recipient email addresses
        /// </summary>
        public List<EmailContract> EmailAddressCcRecipients { get; init; } = new();

        /// <summary>
        ///  Email attachments
        /// </summary>
        public List<EmailAttachmentContract> EmailAttachments { get; init; } = new();

        /// <summary>
        ///  Email headers can be added as a collection of key-value strings
        /// </summary>
        public List<EmailHeaderContract> EmailHeaders { get; init; } = new();

        /// <summary>
        ///  The business domain this request is associated with
        /// </summary>
        public string BusinessDomainType { get; init; } = string.Empty;

        /// <summary>
        ///  The recipients of this email notification
        /// </summary>
        public List<EmailContract> EmailRecipients { get; init; } = new();

        /// <summary>
        ///  The intended email address senders reply-to details
        /// </summary>
        public EmailContract EmailReplyTo { get; init; } = new();

        /// <summary>
        ///  The sender of this email notification
        /// </summary>
        public EmailContract EmailSender { get; init; } = new();

        /// <summary>
        ///  The footer settings contract
        /// </summary>
        public EmailFooterContract Footer { get; init; } = new();

        /// <summary>
        ///  The settings for this email contract
        /// </summary>
        public EmailSettingsContract Settings { get; init; } = new();

        /// <summary>
        ///  The email subject 
        /// </summary>
        public string Subject { get; init; } = string.Empty;

        /// <summary>
        ///  The email template
        /// </summary>
        public EmailTemplateContract Template { get; init; } = new();

        /// <summary>
        ///  Tracking settings
        /// </summary>
        public EmailTrackingContract Tracking { get; init; } = new();

        /// <summary>
        ///  The version of this contract
        /// </summary>
        public override int Version => 1;
    }
}