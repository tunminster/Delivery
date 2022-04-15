namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailSettingsContract
    {
        /// <summary>
        ///  The version of this contract
        /// </summary>
        public int Version => 1;
        
        /// <summary>
        ///  Use email notification in sandbox mode
        /// </summary>
        public bool IsSandBoxMode { get; set; }
    }
}