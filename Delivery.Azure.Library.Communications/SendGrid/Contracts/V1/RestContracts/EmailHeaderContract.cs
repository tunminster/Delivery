namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailHeaderContract
    {
        /// <summary>
        ///  The version of this contract
        /// </summary>
        public int Version => 1;
        
        /// <summary>
        ///  The email header key
        /// </summary>
        public string? Key { get; init; }
        
        /// <summary>
        ///  The email header key-value
        /// </summary>
        public string? Value { get; init; }
    }
}