using System;
using System.IO;

namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailAttachmentContract
    {
        /// <summary>
        ///  Version of this contract
        /// </summary>
        public int Version => 1;
        
        /// <summary>
        ///  Stream of attachment
        /// </summary>
        public Stream?  Stream { get; init; }
        
        /// <summary>
        ///  The date time of the attachment generated 
        /// </summary>
        public DateTimeOffset DateAdded { get; init; }

        /// <summary>
        ///  This is used when the disposition is set to "inline"
        ///  and the attachment is an image, allowing the file to be displayed within the body of your email
        /// </summary>
        public string ContentId { get; init; } = string.Empty;

        /// <summary>
        ///  Specify hwo you would like the attachment to be displayed
        /// </summary>
        public string Disposition { get; init; } = string.Empty;

        /// <summary>
        ///  Filename of attachment
        /// </summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>
        ///  Mime type of attachment
        /// </summary>
        public string Type { get; init; } = string.Empty;
    }
}