using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Driver.Domain.Services;
using Delivery.Driver.Domain.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Driver controller
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DriversController : Controller
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Driver controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DriversController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Create driver application
        /// </summary>
        /// <param name="driverCreationContract"></param>
        /// <param name="driverImage"></param>
        /// <param name="drivingLicenseFrontImage"></param>
        /// <param name="drivingLicenseBackImage"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Post_RegisterDriverAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] DriverCreationContract driverCreationContract, 
            IFormFile? driverImage, IFormFile? drivingLicenseFrontImage, IFormFile? drivingLicenseBackImage)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var validationResult = await new DriverCreationValidator().ValidateAsync(driverCreationContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            // upload image service
            var driverImageCreationContract = new DriverImageCreationContract
            {
                DriverName = driverCreationContract.FullName,
                DriverEmailAddress = driverCreationContract.EmailAddress,
                DriverImage = driverImage,
                DrivingLicenseFrontImage = drivingLicenseFrontImage,
                DrivingLicenseBackImage = drivingLicenseBackImage
            };

            var driverImageCreationStatusContract = await new DriverService(serviceProvider, executingRequestContextAdapter)
                .UploadDriverImagesAsync(driverImageCreationContract);

            var driverCreationStatusContract = new DriverCreationStatusContract
            {
                DateCreated = DateTimeOffset.UtcNow,
                Message = "Driver application submitted successfully.",
                ImageUri = driverImageCreationStatusContract.DriverImageUri,
                DrivingLicenseFrontUri = driverImageCreationStatusContract.DrivingLicenseFrontImageUri,
                DrivingLicenseBackUri = driverImageCreationStatusContract.DrivingLicenseBackImageUri
            };

            var driverCreationMessage = new DriverCreationMessageContract
            {
                PayloadIn = driverCreationContract,
                PayloadOut = driverCreationStatusContract
            };
            
            await new DriverCreationMessagePublisher(serviceProvider).PublishAsync(driverCreationMessage);
            
            return Ok(driverCreationStatusContract);
        }
    }
}