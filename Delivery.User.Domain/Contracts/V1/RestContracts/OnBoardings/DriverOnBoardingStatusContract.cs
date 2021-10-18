namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings
{
    public record DriverOnBoardingStatusContract
    {
        public string AccountNumber { get; init; } = string.Empty;
    }
}