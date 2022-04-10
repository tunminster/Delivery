namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailTrackingContract
    {
        /// <summary>
        ///  The version of this contract
        /// </summary>
        public int Version => 1;
        
        /// <summary>
        ///  Track if an email was opened
        /// </summary>
        public bool IsTrackEmailOpeningEnabled { get; init; }
    }
}