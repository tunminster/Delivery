using Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions;

namespace Delivery.Managements.Domain.Contracts.V1.MessageContracts.MeatOptions
{
    /// <summary>
    ///  Contract that sends to message
    /// </summary>
    public record MeatOptionCreationMessageContract : MeatOptionCreationContract
    {
        /// <summary>
        ///  Meat option external id
        /// </summary>
        public string MeatOptionId { get; init; } = string.Empty;
    }
}