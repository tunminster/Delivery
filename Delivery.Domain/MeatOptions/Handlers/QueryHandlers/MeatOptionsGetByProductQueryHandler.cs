using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.MeatOptions.Contracts.V1.RestContracts;
using Delivery.Domain.MeatOptions.Converters;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Domain.MeatOptions.Handlers.QueryHandlers
{
    public record MeatOptionsByProductId(string ProductId) : IQuery<List<MeatOptionContract>>;
    public class MeatOptionsGetByProductQueryHandler : QueryHandler<MeatOptionsByProductId,List<MeatOptionContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public MeatOptionsGetByProductQueryHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        protected override async Task<List<MeatOptionContract>> HandleQueryAsync(MeatOptionsByProductId request)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var product = await databaseContext.Products.FirstAsync(x => x.ExternalId == request.ProductId);
            var meatOptions = await databaseContext
                .MeatOptions.Where(x => x.ProductId == product.Id)
                .Include(x => x.MeatOptionValues)
                .Include(x=> x.Product).ToListAsync();


            return meatOptions.Select(x => x.ConvertToMeatOptionContract()).ToList();
        }
    }
}