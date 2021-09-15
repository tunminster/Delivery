using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Sharding.Sharding;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Database.ContextOptions;
using Delivery.Database.Entities;
using Delivery.Database.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;

namespace Delivery.Database.Context
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
        
        public ApplicationDbContext(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, DbContextOptions dbContextOptions, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(dbContextOptions,operationalStoreOptions)
        {
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
            ServiceProvider = serviceProvider;
        }
        
        
        public static async Task<ApplicationDbContext> CreateAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            return await DatabaseContextFactory.CreateAsync(serviceProvider, executingRequestContextAdapter, (requestContextAdapter, options) => new ApplicationDbContext(serviceProvider, requestContextAdapter, options, new OperationalStoreOptionsDbContext()));
        }
        
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories {get; set;}
        public DbSet<Product> Products { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<StoreType> StoreTypes { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StripePayment> StripePayments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<OpeningHour> OpeningHours { get; set; }
        public DbSet<StorePaymentAccount> StorePaymentAccounts { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<DriverOrder> DriverOrders { get; set; }
        public DbSet<StoreUser> StoreUsers { get; set; }
        public DbSet<NotificationDevice> NotificationDevices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer entity
            modelBuilder.Entity<Customer>().Property(p => p.Username).HasMaxLength(256);
            
            // Category entity
            modelBuilder.Entity<Category>().Property(p => p.CategoryName).HasMaxLength(300);
            modelBuilder.Entity<Category>().Property(p => p.Description).HasMaxLength(4000);

            // Product entity
            modelBuilder.Entity<Product>().Property(p => p.ProductName).HasMaxLength(300);
            modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(4000);
            modelBuilder.Entity<Product>().Property(p => p.Currency).HasMaxLength(50);
            modelBuilder.Entity<Product>().Property(p => p.CurrencySign).HasMaxLength(20);


            modelBuilder.Entity<Order>().Property(p => p.Description).HasMaxLength(300);
            modelBuilder.Entity<Order>().Property(p => p.CurrencyCode).HasMaxLength(15);
            modelBuilder.Entity<Order>().Property(p => p.PaymentType).HasMaxLength(15);
            modelBuilder.Entity<Order>().Property(p => p.PaymentStatus).HasMaxLength(15);
            
            modelBuilder.Entity<Report>().Property(p => p.Subject).HasMaxLength(250);
            modelBuilder.Entity<Report>().Property(p => p.ContactNumber).HasMaxLength(20);
            modelBuilder.Entity<Report>().Property(p => p.ReportCategory).HasMaxLength(20);
            modelBuilder.Entity<Report>().Property(p => p.Message).HasMaxLength(500);
            
            modelBuilder.Entity<OpeningHour>().Property(p => p.DayOfWeek).HasMaxLength(10);
            modelBuilder.Entity<OpeningHour>().Property(p => p.Open).HasMaxLength(10);
            modelBuilder.Entity<OpeningHour>().Property(p => p.Close).HasMaxLength(10);
            modelBuilder.Entity<OpeningHour>().Property(p => p.TimeZone).HasMaxLength(10);
            
            modelBuilder.Entity<NotificationDevice>().Property(p => p.RegistrationId).HasMaxLength(500);
            modelBuilder.Entity<NotificationDevice>().Property(p => p.Platform).HasMaxLength(250);
            modelBuilder.Entity<NotificationDevice>().Property(p => p.Tag).HasMaxLength(250);

            ConfigureIndexes(modelBuilder);
        }
        
        public override int SaveChanges()
        {
            return SaveChanges(acceptAllChangesOnSuccess: true);
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

        public void SetExecutingRequestContextAdapter(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
            ServiceProvider = serviceProvider;
        }
        
        protected void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            ConfigureExternalIdUniqueConstraint(modelBuilder);
        }

        protected bool IsGenerateExternalIdUniqueConstraint(IMutableEntityType entityType)
        {
            return entityType.ClrType.IsSubclassOf(typeof(Entity));
        }

        private void ConfigureExternalIdUniqueConstraint(ModelBuilder modelBuilder)
        {
            var entities = modelBuilder.Model.GetEntityTypes().Where(IsGenerateExternalIdUniqueConstraint).ToList();
            foreach (var entity in entities)
            {
                var externalIdColumn = nameof(Entity.ExternalId);
                modelBuilder.Entity(entity.ClrType).HasIndex(externalIdColumn).IsUnique()
                    .HasDatabaseName("IX_UniqueExternalId");
            }
        }
        
        public IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; private set; }
        protected IServiceProvider ServiceProvider { get; private set; }
        public IShard Shard => ExecutingRequestContextAdapter.GetShard();
    }
}