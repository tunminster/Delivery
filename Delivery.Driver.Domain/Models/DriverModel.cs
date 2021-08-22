namespace Delivery.Driver.Domain.Models
{
    public record DriverModel
    {
        public int Id { get; init; }

        public string ExternalId { get; init; } = string.Empty;

    }
}