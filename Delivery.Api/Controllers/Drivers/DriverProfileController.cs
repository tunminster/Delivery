using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Azure.Library.WebApi.ModelBinders;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverProfile;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverElasticSearch;
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
    [Route("api/v1/driver-profile", Name = "2 - Driver Profile")]
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

        [Route("update-service-area", Order = 3)]
        [HttpPut]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateDriverServiceAreaAsync(
            DriverServiceAreaUpdateContract driverServiceAreaUpdateContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected authenticated user.")
                .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());

            var validationResult = await new DriverServiceAreaValidator().ValidateAsync(driverServiceAreaUpdateContract);
            
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
                PayloadIn = driverServiceAreaUpdateContract,
                PayloadOut = statusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };
            
            await new DriverServiceAreaUpdateMessagePublisher(serviceProvider).PublishAsync(driverServiceAreaUpdateMessage);

            return Ok(statusContract);
        }

        [Route("get-total-earnings-details" , Order = 4)]
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
        /// <returns></returns>
        [Route("index-driver" , Order = 5)]
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
                .Handle(driverIndexCommand);
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
        [Route("update-driver-profile" , Order = 6)]
        [HttpPut]
        [ProducesResponseType(typeof(StatusContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateDriverProfile_Async([ModelBinder(BinderType = typeof(JsonModelBinder))] DriverProfileUpdateContract driverProfileUpdateContract, 
            IFormFile? driverImage)
        {
            //todo: add validation and file size limit
            
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            
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
                DriverImage = driverImage,
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
        [Route("update-driver-bank-details" , Order = 7)]
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