using System;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Api.Data;
using Delivery.Api.Domain.Command;
using Delivery.Api.Entities;

namespace Delivery.Api.CommandHandler
{
    public class ReportOrderCommandHandler : ICommandHandler<CreateReportOrderCommand, bool>
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;

        public ReportOrderCommandHandler(
            ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<bool> Handle(CreateReportOrderCommand command)
        {

            var report = _mapper.Map<Report>(command);
            await _appDbContext.AddAsync(report);
            return await _appDbContext.SaveChangesAsync() > 0;
        }
    }
}
