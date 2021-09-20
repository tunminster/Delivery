using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Database.Context;
using Delivery.Azure.Library.Database.Entities.V1;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Database.Extensions;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Database.Entities;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Address = Delivery.Database.Entities.Address;

namespace Delivery.Database.Context
{
    public class PlatformDbContext : ShardedDatabaseContext
    {
        
        public PlatformDbContext(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, DbContextOptions dbContextOptions) : base(serviceProvider, executingRequestContextAdapter, dbContextOptions)
        {
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
            ServiceProvider = serviceProvider;
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
            modelBuilder.Entity<NotificationDevice>().Property(p => p.UserEmail).HasMaxLength(250);
            
            ConfigureIndexes(modelBuilder);

        }
        
        // <summary>
        ///     Creates a new instance using <see cref="ISecretProvider" /> as connection string store
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="executingRequestContextAdapter">Contains details about the current request</param>
        public static async Task<PlatformDbContext> CreateAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            return await DatabaseContextFactory.CreateAsync(serviceProvider, executingRequestContextAdapter, (requestContextAdapter, options) => new PlatformDbContext(serviceProvider, requestContextAdapter, options));
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

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var dependencyName = $"Database-{Shard.Key}";
            var dependencyData = new DependencyData("Live");
            var dependencyTarget = Shard.Key;
            
            AuditableEntities.AddRange(ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified));
            UpdateExternalIds(AuditableEntities);
            UpdateAuditableDetails(AuditableEntities);
            
            var result = await new DependencyMeasurement(ServiceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.Sql, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(ExecutingRequestContextAdapter.GetTelemetryProperties())
                .TrackAsync(async () => await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));

            return result;
        }
        
        public IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; }
        protected IServiceProvider ServiceProvider { get; }
        public IShard Shard => ExecutingRequestContextAdapter.GetShard();

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

        private void UpdateExternalIds(List<EntityEntry> entries)
        {
            var entities = entries.Select(entry => entry.Entity).OfType<IEntity>();
            
            foreach (var entity in entities)
            {
                if (string.IsNullOrEmpty(entity.ExternalId))
                {
                    entity.ExternalId = ExecutingRequestContextAdapter.GetShard().GenerateExternalId();
                }
            }
        }
        
        private void UpdateAuditableDetails(List<EntityEntry> entries)
        {
            var entityInsertionDateTime = DateTimeOffset.UtcNow;
            var auditableEntities = entries.Select(entry => entry.Entity).OfType<IAuditableEntity>();
            foreach (var entity in auditableEntities)
            {
                entity.InsertionDateTime = entityInsertionDateTime;

                var entityInsertedBy = ExecutingRequestContextAdapter.GetAuthenticatedUser().UserEmail;
                if (string.IsNullOrEmpty(entityInsertedBy))
                {
                    throw new InvalidOperationException("Entities require to know the authenticated user").WithTelemetry(ExecutingRequestContextAdapter.GetTelemetryProperties());
                }

                entity.InsertedBy = entityInsertedBy;
            }
        }
        
        internal List<EntityEntry> AuditableEntities { get; } = new();
    }
}