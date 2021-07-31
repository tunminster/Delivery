using Microsoft.AspNetCore.Http;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver image creation contract
    /// </summary>
    public record DriverImageCreationContract
    {
        /// <summary>
        ///  Driver name
        /// </summary>
        public string DriverName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver email address
        /// </summary>
        public string DriverEmailAddress { get; init; } = string.Empty;
        
        /// <summary>
        /// Driver main image
        /// </summary>
        public IFormFile? DriverImage { get; init; }
        
        /// <summary>
        ///  Driver license front image
        /// </summary>
        public IFormFile? DrivingLicenseFrontImage { get; init; }
        
        /// <summary>
        ///  Driver license back image
        /// </summary>
        public IFormFile? DrivingLicenseBackImage { get; init; }
    }
}