namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.Supports
{
    /// <summary>
    ///  Support creation contract
    /// </summary>
    public record SupportCreationContract
    {
        /// <summary>
        ///  Name
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        ///  Subject
        /// </summary>
        public string Subject { get; init; } = string.Empty;
        
        /// <summary>
        ///  Message
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        ///  Contact number
        /// </summary>
        public string ContactNumber { get; init; } = string.Empty;
    }
}