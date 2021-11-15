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
using Delivery.Notifications.Model;
using Delivery.Shop.Domain.Handlers.CommandHandlers.ShopNotification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers.Shops
{
    /// <summary>
    ///  Shop notification controller
    /// </summary>
    [Route("api/v1/shop-owner/shop-notification", Name = "4 - Shop Notification")]
    [PlatformSwaggerCategory(ApiCategory.ShopOwner)]
    [ApiController]
    [Authorize(Policy = "ShopApiUser")]
    public class ShopNotificationController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Notification controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ShopNotificationController(
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

            var command = new ShopNotificationRegistrationIdCommand(handle);

            var registrationId =
                await  new ShopNotificationRegistrationIdCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(command);
            
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

            var command = new ShopNotificationRegisterDeviceCommand(registerDeviceModel);
            var deviceRegistrationResponseContract =
                await new ShopNotificationRegisterDeviceCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(command);
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

            var command = new ShopNotificationSendToUserCommand(notificationRequestContract);

            await new ShopNotificationSendToUserCommandHandler(serviceProvider, executingRequestContextAdapter)
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