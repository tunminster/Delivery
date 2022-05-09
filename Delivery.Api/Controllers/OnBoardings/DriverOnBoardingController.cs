using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.Controllers.Drivers;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Database.Models;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverCheckEmailVerification;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverCheckEmailVerification;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverEmailVerification;
using Delivery.Driver.Domain.Handlers.MessageHandlers;
using Delivery.Driver.Domain.Services;
using Delivery.Driver.Domain.Validators;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings;
using Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.Driver;
using Delivery.User.Domain.Validators.OnBoardings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Delivery.Api.Controllers.OnBoardings
{
    [Route("api/v1/on-boarding/delivery-partner" , Name = "1 - Driver OnBoarding api")]
    [PlatformSwaggerCategory(ApiCategory.WebApp)]
    [ApiController]
    public class DriverOnBoardingController : DriverBaseController
    {
        private readonly IServiceProvider serviceProvider;
        public DriverOnBoardingController(IServiceProvider serviceProvider, IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, 
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, IJwtFactory jwtFactory, ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, ILogger<UserManager<ApplicationUser>> logger) 
            : base(serviceProvider, optionsAccessor, passwordHasher, userValidators, passwordValidators, jwtFactory, keyNormalizer, errors, logger)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///  Driver on boarding driver
        /// </summary>
        /// <returns></returns>
        [Route("on-boarding", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(DriverOnBoardingStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post_OnBoardingDriverAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] DriverOnBoardingCreationContract driverOnBoardingCreationContract, string id, IFormFile identityFile)
        {
            var validationResult = await new DriverOnBoardingCreationValidator().ValidateAsync(driverOnBoardingCreationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            if (identityFile.Length < 1)
            {
                return "Identity file should be attached".ConvertToBadRequest();
            }
            
            return Ok(new DriverOnBoardingStatusContract { Message = "Thank you for joining to Delivery partner. We will contact you soon."});
        }

        [Route("register", Order = 2)]
        [HttpPost]
        [ProducesResponseType(typeof(DriverOnBoardingStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post_RegisterAsync(
            [ModelBinder(BinderType = typeof(JsonModelBinder))] DriverCreationContract driverCreationContract,
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

            if(!await CreateUserAsync(driverCreationContract, executingRequestContextAdapter, validationResult))
            {
                return validationResult.ConvertToBadRequest();
            }

            var driverCreationStatusContract = new DriverCreationStatusContract
            {
                DriverId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                DateCreated = DateTimeOffset.UtcNow,
                Message = "Driver application submitted successfully.",
                ImageUri = driverImageCreationStatusContract.DriverImageUri,
                DrivingLicenseFrontUri = driverImageCreationStatusContract.DrivingLicenseFrontImageUri,
                DrivingLicenseBackUri = driverImageCreationStatusContract.DrivingLicenseBackImageUri
            };

            var driverCreationMessage = new DriverCreationMessageContract
            {
                PayloadIn = driverCreationContract,
                PayloadOut = driverCreationStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverCreationMessagePublisher(serviceProvider).PublishAsync(driverCreationMessage);
            
            return Ok(driverCreationStatusContract);
        }
        
        /// <summary>
        ///  Request email verification
        /// </summary>
        /// <returns></returns>
        [Route("request-email-otp", Order = 3)]
        [ProducesResponseType(typeof(DriverEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_RequestEmailOtpAsync(
            [FromBody] DriverStartEmailVerificationContract driverStartEmailVerificationContract)
        {
            var validationResult = await new DriverStartEmailVerificationValidator().ValidateAsync(driverStartEmailVerificationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverStartEmailVerificationStatusContract =
                await new DriverStartEmailVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new DriverStartEmailVerificationCommand(driverStartEmailVerificationContract));

            return Ok(driverStartEmailVerificationStatusContract);
        }
        
        /// <summary>
        ///  Verify email verification 
        /// </summary>
        /// <returns></returns>
        [Route("verify-email-otp", Order = 4)]
        [ProducesResponseType(typeof(DriverEmailVerificationStatusContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post_VerifyEmailOtpAsync(
            [FromBody] DriverCheckEmailVerificationContract driverCheckEmailVerificationContract)
        {
            var validationResult = await new DriverCheckEmailVerificationValidator().ValidateAsync(driverCheckEmailVerificationContract);
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var driverEmailVerificationStatusContract =
                await new DriverCheckEmailVerificationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new DriverCheckEmailVerificationCommand(driverCheckEmailVerificationContract));

            if (driverEmailVerificationStatusContract.Status == "approved")
            {
                //await ConfirmEmailAsync(driverCheckEmailVerificationContract, executingRequestContextAdapter);
            }

            return Ok(driverEmailVerificationStatusContract);
        }

        
    }
}