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
        
        public ApplicationDbContext(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, DbContextOptions dbContextOptions,IOptions<OperationalStoreOptions> operationalStoreOptions) : base(dbContextOptions, operationalStoreOptions)
        {
            ExecutingRequestContextAdapter = executingRequestContextAdapter;
            ServiceProvider = serviceProvider;
        }

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
            modelBuilder.Entity<Order>().Property(p => p.PaymentCard).HasMaxLength(25);
            modelBuilder.Entity<Order>().Property(p => p.PaymentStatus).HasMaxLength(15);
            modelBuilder.Entity<Order>().Property(p => p.OrderStatus).HasMaxLength(15);
            modelBuilder.Entity<Order>().Property(p => p.PaymentOrderCodeRef).HasMaxLength(50);

            modelBuilder.Entity<PaymentCard>().Property(p => p.Token).HasMaxLength(1000);
            modelBuilder.Entity<PaymentCard>().Property(p => p.Name).HasMaxLength(150);
            modelBuilder.Entity<PaymentCard>().Property(p => p.CardType).HasMaxLength(30);
            modelBuilder.Entity<PaymentCard>().Property(p => p.MaskedCardNumber).HasMaxLength(30);
            modelBuilder.Entity<PaymentCard>().Property(p => p.ExpiryMonth).HasMaxLength(10);
            modelBuilder.Entity<PaymentCard>().Property(p => p.ExpiryYear).HasMaxLength(10);

            modelBuilder.Entity<PaymentResponse>().Property(p => p.OrderCode).HasMaxLength(250);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.Token).HasMaxLength(250);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.OrderDescription).HasMaxLength(250);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.Amount).HasMaxLength(20);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.CurrencyCode).HasMaxLength(10);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.PaymentStatus).HasMaxLength(10);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.MaskedCardNumber).HasMaxLength(30);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.CvcResultCode).HasMaxLength(10);
            modelBuilder.Entity<PaymentResponse>().Property(p => p.Environment).HasMaxLength(10);

            modelBuilder.Entity<Report>().Property(p => p.Subject).HasMaxLength(250);
            modelBuilder.Entity<Report>().Property(p => p.ContactNumber).HasMaxLength(20);
            modelBuilder.Entity<Report>().Property(p => p.ReportCategory).HasMaxLength(20);
            modelBuilder.Entity<Report>().Property(p => p.Message).HasMaxLength(500);

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories {get; set;}
        public DbSet<Product> Products { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<PaymentCard> PaymentCards { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentResponse> PaymentResponses { get; set; }
        public DbSet<Report> Reports { get; set; }
        
        // <summary>
        ///     Creates a new instance using <see cref="ISecretProvider" /> as connection string store
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="executingRequestContextAdapter">Contains details about the current request</param>
        public static async Task<ApplicationDbContext> CreateAsync(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            return await DatabaseContextFactory.CreateAsync(serviceProvider, executingRequestContextAdapter, (requestContextAdapter, options) => new ApplicationDbContext(serviceProvider, requestContextAdapter, options, null));
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

            var result = await new DependencyMeasurement(ServiceProvider)
                .ForDependency(dependencyName, MeasuredDependencyType.Sql, dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(ExecutingRequestContextAdapter.GetTelemetryProperties())
                .TrackAsync(async () => await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));

            return result;
        }
        
        public IExecutingRequestContextAdapter ExecutingRequestContextAdapter { get; }
        protected IServiceProvider ServiceProvider { get; }
        public IShard Shard => ExecutingRequestContextAdapter.GetShard();
    }
}