using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.Constants;
using Delivery.Domain.QueryHandlers;
using Delivery.User.Domain.Contracts.V1.RestContracts.Managements;
using Delivery.User.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.User.Domain.Handlers.QueryHandlers
{
    public record UserGetQuery(string UserEmail) : IQuery<UserProfileContract>;
    
    public class UserGetQueryHandler : IQueryHandler<UserGetQuery, UserProfileContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public UserGetQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<UserProfileContract> Handle(UserGetQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Order>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var userCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{query.UserEmail.ToLowerInvariant()}-{nameof(UserGetQueryHandler).ToLowerInvariant()}";

            var userName = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;
            var role = executingRequestContextAdapter.GetAuthenticatedUser().Role;

            if (role == UserRoleConstants.Administrator && userName != null)
            {
                return new UserProfileContract
                {
                    EmailAddress = userName,
                };
            }

            var userProfileContract = await dataAccess.GetCachedItemsAsync(
                userCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.StoreUsers
                    .Where(x => x.Username == userName)
                    .Select(x => x.ConvertToUserProfileContract())
                    .SingleAsync()
                );

            if (userProfileContract == null)
            {
                throw new InvalidOperationException("Expected user profile contract");
            }
            
            return userProfileContract;
        }
    }
}