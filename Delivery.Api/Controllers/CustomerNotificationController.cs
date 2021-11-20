using System;
using System.Net;
using System.Threading.Tasks;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Azure.Library.NotificationHub.Contracts.V1;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.WebApi.Contracts;
using Delivery.Azure.Library.WebApi.Extensions;
using Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification;
using Delivery.Customer.Domain.Handlers.CommandHandlers.CustomerNotification;
using Delivery.Customer.Domain.Handlers.CommandHandlers.PushNotifications;
using Delivery.Customer.Domain.Validators;
using Delivery.Domain.FrameWork.Context;
using Delivery.Notifications.Contracts.V1.Enums;
using Delivery.Notifications.Contracts.V1.RestContracts;
using Delivery.Notifications.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.Api.Controllers
{
    /// <summary>
    ///  Customer controller
    /// </summary>
    [Route("api/customer-notification", Name = "12 - Customer")]
    [PlatformSwaggerCategory(ApiCategory.Customer)]
    [ApiController]
    [Authorize(Policy = "CustomerApiUser")]
    public class CustomerNotificationController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        ///  Notification controller
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CustomerNotificationController(
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

            var command = new CustomerNotificationRegistrationIdCommand(handle);

            var registrationId =
                await  new CustomerNotificationRegistrationIdCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(command);
            
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

            var command = new CustomerNotificationRegisterDeviceCommand(registerDeviceModel);
            var deviceRegistrationResponseContract =
                await new CustomerNotificationRegisterDeviceCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(command);
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

            var command = new CustomerSendNotificationToUserCommand(notificationRequestContract);

            await new CustomerSendNotificationToUserCommandHandler(serviceProvider, executingRequestContextAdapter)
                .Handle(command);

            var notificationResponseContract = new NotificationResponseContract
            {
                Status = NotificationStatus.Created,
                DateCreated = DateTimeOffset.UtcNow
            };

            return Ok(notificationResponseContract);
        }
        
        /// <summary>
        ///  Send order arrive notification
        /// </summary>
        /// <returns></returns>
        [Route("send/order-message", Order = 5)]
        [HttpPost]
        [ProducesResponseType(typeof(NotificationResponseContract), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestContract), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SendOrderArrivedPushNotificationAsync(CustomerOrderNotificationRequestContract customerOrderNotificationRequestContract)
        {
            var executingRequestContextAdapter = Request.GetExecutingRequestContextAdapter();
            
            var validationResult = await new CustomerOrderNotificationRequestContractValidator().ValidateAsync(customerOrderNotificationRequestContract);
            
            if (!validationResult.IsValid)
            {
                return validationResult.ConvertToBadRequest();
            }

            var command = new CustomerOrderPushNotificationCommand(customerOrderNotificationRequestContract.OrderId, customerOrderNotificationRequestContract.Filter);

            await new CustomerOrderPushNotificationCommandHandler(serviceProvider, executingRequestContextAdapter)
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