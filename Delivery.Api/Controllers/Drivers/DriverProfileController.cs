using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.Files;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Azure.Library.WebApi.Swagger.Attributes;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Configurations;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverProfile;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverIndex;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverProfile;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverProfile;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile;
using Delivery.Driver.Domain.Handlers.QueryHandlers.DriverStatus;
using Delivery.Driver.Domain.Services;
using Delivery.Driver.Domain.Validators.DriverProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Drivers
{
    /// <summary>
    ///  Driver controller
    /// </summary>
    [Route("api/v1/delivery-partners/driver-profile", Name = "2 - Driver Profile")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [ApiController]
    [Authorize(Policy = "DriverApiUser")]
    public class DriverProfileController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public DriverProfileController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Get store type
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("get-profile", Order = 1)]
        [HttpGet]
        [ProducesResponseType(typeof(DriverProfileContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDriverProfileAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverProfileQuery = new DriverProfileQuery
            {
                EmailAddress = userEmail
            };

            var driverProfileContract = await new DriverProfileQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverProfileQuery);

            return Ok(driverProfileContract);
        }

        /// <summary>
        ///  Get driver status
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("get-driver-status", Order = 2)]
        [HttpGet]
        [ProducesResponseType(typeof(DriverActiveStatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDriverStatusAsync(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverStatusQuery = new DriverStatusQuery(userEmail);
            var driverActiveStatusContract = await new DriverStatusQueryHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(driverStatusQuery);

            return Ok(driverActiveStatusContract);
        }

        /// <summary>
        ///  Get service area
        /// </summary>
        /// <remarks>
        ///     Get service area endpoint allows user to get current service area location
        /// </remarks>
        [Route("get-service-area", Order = 3)]
        [HttpGet]
        [ProducesResponseType(typeof(DriverServiceAreaContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_DriverServiceAreaAsync()
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverServiceAreaContract =
                await new DriverServiceAreaGetQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new DriverServiceAreaGetQuery(userEmail));

            return Ok(driverServiceAreaContract);
        }

        /// <summary>
        ///  Update service area contract
        /// </summary>
        /// <remarks>
        ///     The update service area endpoint allows user to update the service area
        /// </remarks>
        [Route("update-service-area", Order = 4)]
        [HttpPut]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateDriverServiceAreaAsync(
            DriverServiceAreaContract driverServiceAreaContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var validationResult = await new DriverServiceAreaValidator().ValidateAsync(driverServiceAreaContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }
            
            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            var driverServiceAreaUpdateMessage = new DriverServiceAreaUpdateMessageContract
            {
                PayloadIn = driverServiceAreaContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverServiceAreaUpdateMessagePublisher(serviceProvider).PublishAsync(driverServiceAreaUpdateMessage);

            return Ok(statusContract);
        }

        /// <summary>
        ///  Get total earnings details
        /// </summary>
        /// <remarks>
        ///     The get total earnings details endpoint allows user to get total earning details.
        /// </remarks>
        [Route("get-total-earnings-details" , Order = 5)]
        [HttpGet]
        [ProducesResponseType(typeof(DriverTotalEarningsContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get_Total_Earnings_Async(CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverTotalEarningsQuery = new DriverTotalEarningsQuery
            {
                UserEmail = userEmail
            };

            var driverTotalEarningsContract =
                await new DriverTotalEarningsQueryHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(driverTotalEarningsQuery);

            return Ok(driverTotalEarningsContract);
        }
        
        /// <summary>
        ///  Index driver
        /// </summary>
        /// <remarks>
        ///  The index driver endpoint allows user to update driver index
        /// </remarks>
        [Route("index-driver" , Order = 6)]
        [HttpPost]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> IndexDriverAsync(DriverIndexCreationContract driverIndexCreationContract, CancellationToken cancellationToken = default)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverIndexCommand = new DriverIndexCommand(driverIndexCreationContract.DriverId);
            var driverIndexStatusContract = await new DriverIndexCommandHandler(serviceProvider, executingRequestContextAdapter)
                .HandleAsync(driverIndexCommand);
            var statusContract = new StatusContract
            {
                Status = driverIndexStatusContract.Status,
                DateCreated = driverIndexStatusContract.DateCreated
            };
            
            return Ok(statusContract);
        }
        
        /// <summary>
        ///  Upload profile image
        /// </summary>
        /// <returns></returns>
        [Route("update-driver-profile" , Order = 7)]
        [HttpPut]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        [RequestSizeLimit(5000000)]
        [OpenApiMultipartUploadFilter("driverImage")]
        public async Task<IActionResult> UpdateDriverProfile_Async([ModelBinder(BinderType = typeof(JsonModelBinder))] DriverProfileUpdateContract driverProfileUpdateContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var files = await Request.GetFilesAsync();
            
            var driverFileConfigurations = new DriverFileConfigurations(serviceProvider);
            List<string> fileExtensions = driverFileConfigurations.GetValidFileExtensions();
            var totalMaximumFilesSize = driverFileConfigurations.MaximumTotalFilesSize;

            var fileValidationResult =
                await new DriverProfileFileUploadValidator(fileExtensions, totalMaximumFilesSize).ValidateAsync(files);
            
            if (!fileValidationResult.IsValid)
            {
                return fileValidationResult.ConvertToBadRequest();
            }
            
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var driverProfileQuery = new DriverProfileQuery
            {
                EmailAddress = userEmail
            };

            var driverProfileContract =
                await new DriverProfileQueryHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    driverProfileQuery);
            
            
            // upload image service
            var driverImageCreationContract = new DriverImageCreationContract
            {
                DriverName = driverProfileContract.FullName,
                DriverEmailAddress = driverProfileContract.EmailAddress,
                DriverImage = files.First(),
                DrivingLicenseFrontImage = null,
                DrivingLicenseBackImage = null
            };
            
            var driverImageCreationStatusContract = await new DriverService(serviceProvider, executingRequestContextAdapter)
                .UploadDriverImagesAsync(driverImageCreationContract);

            var driverProfileUpdateCommand = new DriverProfileUpdateCommand(driverProfileUpdateContract,
                driverImageCreationStatusContract.DriverImageUri);

            await new DriverProfileUpdateCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                driverProfileUpdateCommand);
            
            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
            return Ok(statusContract);
        }
        
        /// <summary>
        ///  Upload profile image
        /// </summary>
        /// <returns></returns>
        [Route("update-driver-bank-details" , Order = 8)]
        [HttpPut]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateDriverProfile_Async(DriverProfileBankDetailsContract driverProfileBankDetailsContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new DriverProfileBankDetailsValidator().ValidateAsync(driverProfileBankDetailsContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var driverProfileBankDetailsCommand = new DriverProfileBankDetailsCommand(driverProfileBankDetailsContract);

            await new DriverProfileBankDetailsCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                driverProfileBankDetailsCommand);
            
            var statusContract = new DriverProfileBankDetailsStatusContract
            {
                Status = true,
                Message = "We will contact you to verify the bank details.",
                DateCreated = DateTimeOffset.UtcNow
            };
            
            return Ok(statusContract);
        }
        
        
    }
}