using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    public record DriverCreationStatusContract
    {
        public DateTimeOffset DateCreated { get; init; }

        public string Message { get; init; } = string.Empty;

        public string ImageUri { get; init; } = string.Empty;

        public string DrivingLicenseFrontUri { get; init; } = string.Empty;

        public string DrivingLicenseBackUri { get; init; } = string.Empty;
    }
}