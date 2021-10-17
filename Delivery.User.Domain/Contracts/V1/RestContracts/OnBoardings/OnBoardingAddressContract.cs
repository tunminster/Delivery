namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings
{
    public record OnBoardingAddressContract
    {
        public string AddressLine1 { get; init; } = string.Empty;

        public string AddressLine2 { get; init; } = string.Empty;

        public string City { get; init; } = string.Empty;

        public string County { get; init; } = string.Empty;

        public string Country { get; init; } = string.Empty;

        public string PostalCode { get; init; } = string.Empty;
    }
}