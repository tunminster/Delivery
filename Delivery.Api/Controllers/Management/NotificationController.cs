using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.NotificationHub.Contracts.V1;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Domain.FrameWork.Context;
using Delivery.Notifications.Contracts.V1.Enums;
using Delivery.Notifications.Contracts.V1.RestContracts;
using Delivery.Notifications.Handlers.CommandHandlers.CreateRegistrationId;
using Delivery.Notifications.Handlers.CommandHandlers.DeleteRegisterDevice;
using Delivery.Notifications.Handlers.CommandHandlers.RegisterDevice;
using Delivery.Notifications.Handlers.CommandHandlers.SendNotificationToUser;
using Delivery.Notifications.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Management
{
    /// <summary>
    ///  Notification controller
    /// </summary>
    [Route("api/v1/management/notification" , Name = "1 - Notification management")]
    [PlatformSwaggerCategory(ApiCategory.Driver)]
    [ApiController]
    [Authorize(Roles = "Driver,ShopOwner")]
    public class NotificationController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Notification controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public NotificationController(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        ///  Create Registration id
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [Route("register", Order = 1)]
        [HttpPost]
        [ProducesResponseType(typeof(DeviceRegistrationResponseContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateRegistrationIdAsync(string handle = null)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var command = new CreateRegistrationIdCommand(handle);

            var registrationId =
               await  new CreateRegistrationIdCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(command);
            
            return Ok(new DeviceRegistrationResponseContract{Id = registrationId});
        }

        /// <summary>
        ///  Register Device
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deviceRegistrationContract"></param>
        /// <returns></returns>
        [Route("register", Order = 2)]
        [HttpPut]
        [ProducesResponseType(typeof(DeviceRegistrationResponseContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateOrUpdateRegistrationAsync(string id, DeviceRegistrationContract deviceRegistrationContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var registerDeviceModel = new RegisterDeviceModel
            {
                RegistrationId = id,
                DeviceRegistration = deviceRegistrationContract
            };

            var command = new RegisterDeviceCommand(registerDeviceModel);
            var deviceRegistrationResponseContract =
                await new RegisterDeviceCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(command);
            return Ok(deviceRegistrationResponseContract);
        }

        /// <summary>
        ///  Delete registration
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("register", Order = 3)]
        [HttpDelete]
        [ProducesResponseType(typeof(DeviceRegistrationResponseContract), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteRegistrationAsync(string id)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var command = new DeleteRegisterCommand(id);
            var registrationId = await new DeleteRegisterCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(command);

            var deviceRegistrationResponseContract = new DeviceRegistrationResponseContract
            {
                Id = registrationId
            };
            
            return Ok(deviceRegistrationResponseContract);
        }

        /// <summary>
        ///  Send push notification
        /// </summary>
        /// <param name="notificationRequestContract"></param>
        /// <returns></returns>
        [Route("send/message", Order = 4)]
        [HttpPost]
        [ProducesResponseType(typeof(NotificationResponseContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SendPushNotificationAsync(NotificationRequestContract notificationRequestContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();

            var command = new SendNotificationToUserCommand(notificationRequestContract);

            await new SendNotificationToUserCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(command);

            var notificationResponseContract = new NotificationResponseContract
            {
                Status = NotificationStatus.Created,
                DateCreated = DateTimeOffset.UtcNow
            };

            return Ok(notificationResponseContract);
        }
        
    }
}