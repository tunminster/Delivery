namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings
{
    /// <summary>
    ///  OnBoarding bank account contract
    /// </summary>
    public record OnBoardingBankAccountContract
    {
        /// <summary>
        ///  Account number
        /// </summary>
        /// <example>{{accountNumber}}</example>
        public string AccountNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Routing number
        /// </summary>
        /// <example>{{routingNumber}}</example>
        public string RoutingNumber { get; init; } = string.Empty;
    }
}