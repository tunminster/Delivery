namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    public record DriverLoginContract
    {
        public string Username { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}