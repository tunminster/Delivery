namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailTemplateContract
    {
        /// <summary>
        ///  Dynamic template data
        /// </summary>
        public object? DynamicTemplateData { get; init; }

        /// <summary>
        ///  The version of this contract
        /// </summary>
        public int Version => 1;
        
        /// <summary>
        ///  The template of this contract
        /// </summary>
        public string? TemplateId { get; init; }
    }
}