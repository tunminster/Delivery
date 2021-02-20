using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Order.Domain.Handlers.CommandHandlers
{
    public class ReportOrderCommandHandler : ICommandHandler<CreateReportOrderCommand, bool>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ReportOrderCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<bool> Handle(CreateReportOrderCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var report = new Report
            {
                ContactNumber = command.ReportCreationContract.ContactNumber,
                CustomerId = command.ReportCreationContract.UserId,
                Message = command.ReportCreationContract.Message,
                ReportCategory = command.ReportCreationContract.ReportCategory,
                Subject = command.ReportCreationContract.Subject
            };
            await databaseContext.AddAsync(report);
            return await databaseContext.SaveChangesAsync() > 0;
        }
    }
}