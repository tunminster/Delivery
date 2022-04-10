namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailFooterContract
    {
        /// <summary>
        ///  The version of this contract
        /// </summary>
        public int Version => 1;
        
        /// <summary>
        ///  The email footer html
        /// </summary>
        public string? Html { get; init; }
        
        /// <summary>
        ///  Enable footer settings
        /// </summary>
        public bool IsEnabled { get; init; }
        
        /// <summary>
        /// The email footer text
        /// </summary>
        public string? Text { get; init; }
    }
}