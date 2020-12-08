using System;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Database.Entities;
using Delivery.Database.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Delivery.Database.Context
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        
        public override int SaveChanges()
        {
            return SaveChanges(acceptAllChangesOnSuccess: true);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            throw new NotSupportedException("Use async methods only").WithTelemetry(ExecutingRequestContextAdapter.GetTelemetryProperties());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
        }

        public async Task<int> SaveChangeAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter,
            CancellationToken cancellationToken = new CancellationToken())
        {
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
            ServiceProvider = serviceProvider;
            return await SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var dependencyName = $"Database-{Shard.Key}";
            var dependencyData = new DependencyData("Live");
            var dependencyTarget = Shard.Key;

            var result = await new DependencyMeasurement(ServiceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.Sql, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(ExecutingRequestContextAdapter.GetTelemetryProperties())
                .TrackAsync(async () => await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));

            return result;
        }
        
        public IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; private set; }
        protected IServiceProvider ServiceProvider { get; private set; }
        public IShard Shard => ExecutingRequestContextAdapter.GetShard();
    }
}