using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Command;
using Delivery.Api.Entities;
using Delivery.Api.Models;
using Delivery.Api.Utils;
using Delivery.Api.Utils.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Delivery.Api.CommandHandler
{
    public class OrderCommandHandler : ICommandHandler<CreateOrderCommand, bool>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _clientFactory;
        private WorldPayConfig _worldPayConfig;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderCommandHandler> _logger;

        public OrderCommandHandler(ApplicationDbContext appDbContext,
            IMapper mapper,
            IHttpClientFactory clientFactory,
            IConfiguration Configuration,
            ILogger<OrderCommandHandler> logger
            )
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _clientFactory = clientFactory;
            _configuration = Configuration;
            _worldPayConfig = new WorldPayConfig();
            _configuration.GetSection("WorldPayConfigs").Bind(_worldPayConfig);
            _logger = logger;
        }

        public async Task<bool> Handle(CreateOrderCommand command)
        {

            // save order
            var order = await SaveOrder(command);
            
            // request worldpay payment
            var paymentModel = new WorldPayPaymentModel();
            
            paymentModel.paymentMethod.Type = command.PaymentType;
            paymentModel.paymentMethod.Name = command.CardHolderName;
            paymentModel.paymentMethod.ExpiryMonth = command.PaymentExpiryMonth;
            paymentModel.paymentMethod.ExpiryYear = command.PaymentExpiryYear;
            paymentModel.paymentMethod.CardNumber = command.PaymentCard;
            paymentModel.paymentMethod.Cvc = command.PaymentCVC;
            paymentModel.paymentMethod.IssueNumber = command.PaymentIssueNumber;

            paymentModel.OrderType = WorldPayEnum.ECOM.ToString();
            paymentModel.OrderDescription = $"orderId_{order.Id}_payment_{command.DateCreated.ToString("ddMMyyyyhhmm")}";
            paymentModel.Amount = command.TotalAmount.ToString().Replace(".", "");
            paymentModel.CurrencyCode = CurrencyCodeEnum.GBP.ToString();

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.worldpay.com/v1/orders");
            //request.Headers.Add("Content-type", "application/json");
            request.Headers.Add("Authorization", _worldPayConfig.ServiceKey);
            request.Content = new StringContent(JsonConvert.SerializeObject(paymentModel),Encoding.UTF8,
                "application/json");

            var client = _clientFactory.CreateClient();

            var response =  await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<WorldPayPaymentResponseModel>(result);

                if(String.Equals(responseModel.PaymentStatus, OrderStatusEnum.Success.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    // update order
                    await UpdateOrder(responseModel, order.Id, true);            
                }
                else
                {
                    await UpdateOrder(responseModel, order.Id, false);
                }

                // save payment response
                await SavePaymentReponse(responseModel);

                // save payment card
                if (command.SaveCard && !IsCardSaved(responseModel.PaymentResponse.MaskedCardNumber, command.CustomerId))
                {
                    await AddPaymentCard(responseModel, command);
                }

                return true;

            }
            else
            {
                _logger.LogError($"WorldPay payment api return status code : {response.StatusCode} error for Order Id : {order.Id}");
            }

            return false;
        }

        private async Task<Order> SaveOrder(CreateOrderCommand command)
        {
            var maskedCardNumberWithSpaces = GetMaskedCardNumber(command.PaymentCard);

            var order = new Order();
            order.Description = command.Description;
            order.TotalAmount = command.TotalAmount;
            order.CurrencyCode = command.CurrencyCode;
            order.PaymentType = command.PaymentType;
            order.PaymentCard = maskedCardNumberWithSpaces;
            order.PaymentStatus = PaymentStatusEnum.InProgress.ToString();
            order.OrderStatus = OrderStatusEnum.InProgress.ToString();
            order.CustomerId = command.CustomerId;
            order.DateCreated = command.DateCreated;
            order.AddressId = command.ShippingAddressId;
            order.OrderItems = new List<OrderItem>();

            foreach (var item in command.OrderItems)
            {
                order.OrderItems.Add(new OrderItem()
                {
                    ProductId = item.ProductId,
                    Count = item.Count
                });
            }

            await _appDbContext.AddAsync(order);
            await _appDbContext.SaveChangesAsync();

            return order;
        }

        private async Task AddPaymentCard(WorldPayPaymentResponseModel responseModel, CreateOrderCommand command)
        {
            var paymentCard = new PaymentCard();
            paymentCard.Token = responseModel.Token;
            paymentCard.Name = responseModel.PaymentResponse.Name;
            paymentCard.CardType = responseModel.PaymentResponse.CardType;
            paymentCard.MaskedCardNumber = responseModel.PaymentResponse.MaskedCardNumber;
            paymentCard.ExpiryMonth = responseModel.PaymentResponse.ExpiryMonth;
            paymentCard.ExpiryYear = responseModel.PaymentResponse.ExpiryYear;
            paymentCard.CustomerId = command.CustomerId;
            paymentCard.DateAdded = command.DateCreated;

            await _appDbContext.AddAsync(paymentCard);
            await _appDbContext.SaveChangesAsync();
        }

        private bool IsCardSaved(string maskedCardNumber, int customerId)
        {
            return _appDbContext.PaymentCards.Any(x => x.MaskedCardNumber == maskedCardNumber && x.CustomerId == customerId);
        }

        private async Task UpdateOrder(WorldPayPaymentResponseModel responseModel, int orderId, bool paymentSuccess)
        {
            var updateOrder = _appDbContext.Orders.FirstOrDefault(x => x.Id == orderId);
            updateOrder.PaymentStatus = paymentSuccess ? PaymentStatusEnum.Success.ToString() : PaymentStatusEnum.Failed.ToString();
            updateOrder.PaymentOrderCodeRef = responseModel.OrderCode;

             _appDbContext.Update(updateOrder);

            await _appDbContext.SaveChangesAsync();
        }

        private async Task SavePaymentReponse(WorldPayPaymentResponseModel responseModel)
        {
            var paymentResponse = new Entities.PaymentResponse();
            paymentResponse.OrderCode = responseModel.OrderCode;
            paymentResponse.Token = responseModel.Token;
            paymentResponse.OrderDescription = responseModel.OrderDescription;
            paymentResponse.Amount = decimal.Parse(responseModel.Amount);
            paymentResponse.CurrencyCode = responseModel.CurrencyCode;
            paymentResponse.PaymentStatus = responseModel.PaymentStatus;
            paymentResponse.MaskedCardNumber = responseModel.PaymentResponse.MaskedCardNumber;
            paymentResponse.CvcResultCode = responseModel.ResultCodes.CvcResultCode;
            paymentResponse.Environment = responseModel.Environment;
            paymentResponse.DateAdded = DateTime.UtcNow;

            await _appDbContext.AddAsync(paymentResponse);
            await _appDbContext.SaveChangesAsync();
        }

        public static string GetMaskedCardNumber(string cardNumber)
        {

            var firstDigits = cardNumber.Substring(0, 0);
            var lastDigits = cardNumber.Substring(cardNumber.Length - 4, 4);

            var requiredMask = new String('X', cardNumber.Length - firstDigits.Length - lastDigits.Length);

            var maskedString = string.Concat(firstDigits, requiredMask, lastDigits);
            var maskedCardNumberWithSpaces = Regex.Replace(maskedString, ".{4}", "$0 ");

            return maskedCardNumberWithSpaces;
        }

        
    }
}
