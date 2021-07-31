using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver image creation status contract
    /// </summary>
    public record DriverImageCreationStatusContract
    {
        /// <summary>
        /// Driver image uri
        /// </summary>
        public string DriverImageUri { get; init; } = string.Empty;
        
        /// <summary>
        /// Driver license front image uri
        /// </summary>
        public string DrivingLicenseFrontImageUri { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver license back image uri
        /// </summary>
        public string DrivingLicenseBackImageUri { get; init; } = string.Empty;
    }
}