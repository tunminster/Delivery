namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions
{
    /// <summary>
    ///  Status contact
    /// </summary>
    public record MeatOptionCreationStatusContract
    {
        /// <summary>
        ///  External meat option id
        /// </summary>
        public string MeatOptionId { get; init; } = string.Empty;
    }
}

