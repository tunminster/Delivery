using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Command;

namespace Delivery.Api.CommandHandler
{
    public class PaymentCommandHandler : ICommandHandler<CreateDirectPaymentCommand>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public PaymentCommandHandler(ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public Task Handle(CreateDirectPaymentCommand command)
        {
            throw new NotImplementedException();
        }

        public void Handles(CreateDirectPaymentCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
