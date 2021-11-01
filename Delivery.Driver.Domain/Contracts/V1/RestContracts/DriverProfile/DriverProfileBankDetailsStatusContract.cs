using Delivery.Domain.Contracts.V1.RestContracts;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile
{
    /// <summary>
    ///  Driver profile bank details status contract
    /// </summary>
    public record DriverProfileBankDetailsStatusContract : StatusContract
    {
        /// <summary>
        ///  Message
        /// </summary>
        /// <example>{{message}}</example>
        public string Message { get; init; } = string.Empty;
    }
}