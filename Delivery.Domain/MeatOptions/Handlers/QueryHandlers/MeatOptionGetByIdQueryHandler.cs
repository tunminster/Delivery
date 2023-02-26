using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.MeatOptions.Contracts.V1.RestContracts;
using Delivery.Domain.MeatOptions.Converters;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Domain.MeatOptions.Handlers.QueryHandlers
{
    public record MeatOptionGetByIdQuery(string MeatOptionId) : IQuery<MeatOptionContract>;
    
    public class MeatOptionGetByIdQueryHandler : QueryHandler<MeatOptionGetByIdQuery,MeatOptionContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public MeatOptionGetByIdQueryHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        protected override async Task<MeatOptionContract> HandleQueryAsync(MeatOptionGetByIdQuery request)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var meatOption = await databaseContext.MeatOptions.FirstAsync(x => x.ExternalId == request.MeatOptionId);
            return meatOption.ConvertToMeatOptionContract();
        }
    }

}