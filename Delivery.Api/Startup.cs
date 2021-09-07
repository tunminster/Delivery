using System;
using System.Text;
using AutoMapper;
using Delivery.Api.Models;
using Delivery.Api.OpenApi;
using Delivery.Api.OpenApi.Enums;
using Delivery.Api.Utils.Configs;
using Delivery.Azure.Library.Caching.Cache;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Environments;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Stdout;
using Delivery.Azure.Library.WebApi.Middleware;
using Delivery.Database.Context;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Features;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Delivery.Azure.Library.ConnectionManagement.HostedServices;
using Delivery.Azure.Library.KeyVault.Providers;
using Delivery.Azure.Library.Messaging.HostedServices;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections;
using Delivery.Azure.Library.Messaging.ServiceBus.Connections.Interfaces;
using Delivery.Azure.Library.Microservices.Hosting.HostedServices;
using Delivery.Azure.Library.Resiliency.Stability;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Storage.Blob.Connections;
using Delivery.Azure.Library.Storage.Blob.Interfaces;
using Delivery.Azure.Library.Storage.Cosmos.Connections;
using Delivery.Azure.Library.Storage.Cosmos.Interfaces;
using Delivery.Azure.Library.Storage.HostedServices;
using Delivery.Azure.Library.WebApi.Conventions;
using Delivery.Azure.Library.WebApi.Filters;
using Delivery.Database.Models;
using Delivery.Database.Seeding;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.HttpClients.Extensions;
using Delivery.Domain.Models;
using Delivery.Store.Domain.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Delivery.Api
{
    public class Startup
    {
        private const string SecretKey = "iNivDmHLpUA223sqsfhqGbMRdRj1PVkH"; // todo: get this from somewhere secure
        private readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddHealthChecks();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DeliveryDevConnection")));
            
            services.AddIdentity<Database.Models.ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(2));
            
            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });
            
#if DEBUG
            // log the local debug information to the output window for easier testing
            services.AddSingleton<IApplicationInsightsTelemetry, StdoutApplicationInsightsTelemetry>(provider => new StdoutApplicationInsightsTelemetry(provider, Configuration.GetValue<string>("Service_Name")));
#else
			services.AddSingleton<IApplicationInsightsTelemetry, Delivery.Azure.Library.Telemetry.ApplicationInsights.ApplicationInsightsTelemetry>(provider => new Delivery.Azure.Library.Telemetry.ApplicationInsights.ApplicationInsightsTelemetry(provider, Configuration.GetValue<string>("Service_Name"), new Delivery.Azure.Library.Telemetry.ApplicationInsights.Initializers.ApplicationTelemetryInitializers(provider, typeof(Startup).Assembly)));
#endif

            services.AddSingleton<IJwtFactory, JwtFactory>();


            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();

            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            }).AddFacebook(options =>
            {
                options.AppId = Configuration["Authentication:Facebook:AppId"];
                options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            });
            
            

            // api user claim policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(x =>
                            x.User.HasClaim(ClaimData.JwtClaimIdentifyClaim.ClaimType.ToLower(), ClaimData.JwtClaimIdentifyClaim.ClaimValue)));
                
                options.AddPolicy("BackOfficeUser", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(x => 
                            x.User.HasClaim(ClaimData.OrderPageAccess.ClaimType, ClaimData.OrderPageAccess.ClaimValue)));
                
                options.AddPolicy("ShopApiUser", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(x =>
                            x.User.HasClaim(ClaimData.ShopApiAccess.ClaimType, ClaimData.ShopApiAccess.ClaimValue)));
                
                options.AddPolicy("DriverApiUser", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(x =>
                            x.User.HasClaim(ClaimData.DriverApiAccess.ClaimType, ClaimData.DriverApiAccess.ClaimValue)));
                    //policy.RequireClaim(ClaimData.JwtClaimIdentifyClaim.ClaimType, ClaimData.JwtClaimIdentifyClaim.ClaimValue));
            });

            // add identity
            var builder = services.AddIdentityCore<Database.Models.ApplicationUser>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            
            services.AddResponseCaching();
            
            services.AddPlatformSwaggerServices<ApplicationSwaggerConfiguration>(PlatformHttpClientExtensions
                .GetApiScopes<ApiCategory>());

            services.AddAutoMapper(typeof(Startup));
            services.Configure<AzureStorageConfig>(Configuration.GetSection("AzureStorageConfig"));
            services.Configure<AzureLogConfig>(Configuration.GetSection("AzureLogConfig"));
            services.Configure<WorldPayConfig>(Configuration.GetSection("WorldPayConfigs"));

            services.AddControllers(options =>
            {
                options.Filters.Add(new ApiLoggingFilterAttribute());
                options.Filters.Add(new JsonPayloadSchemaAttribute());
            });

            services.AddHttpClient();
            
            // initial identity data seeding
            services.AddTransient<IdentityData>();

            //register handlers
            services.AddSingleton<IEnvironmentProvider, EnvironmentProvider>();
            services.AddSingleton<Delivery.Azure.Library.Configuration.Configurations.Interfaces.IConfigurationProvider, Delivery.Azure.Library.Configuration.Configurations.ConfigurationProvider>();
            services.AddSingleton<ISecretProvider, KeyVaultCachedSecretProvider>();
            services.AddSingleton<ICircuitManager, CircuitManager>();
            services.AddSingleton<IFeatureProvider, FeatureProvider>();
            services.AddSingleton<ICosmosDatabaseConnectionManager, CosmosDatabaseConnectionManager>();
            services.AddSingleton<IServiceBusSenderConnectionManager, ServiceBusSenderConnectionManager>();
            services.AddSingleton<IBlobStorageConnectionManager, BlobStorageConnectionManager>();
            
            var useInMemory = Configuration.GetValue<bool?>("Test_Use_In_Memory");
            if (useInMemory.GetValueOrDefault())
            {
                services.AddSingleton<IManagedCache, ManagedMemoryCache>();
            }
            else
            {
                services.AddPlatformCaching();
                services.AddSingleton<IManagedCache, ManagedRedisCache>();
            }

            services.AddHostedService(serviceProvider => new MultipleTasksBackgroundService(
                new QueueServiceBusWorkBackgroundService(serviceProvider),
                new QueueBlobUploadWorkBackgroundService(serviceProvider),
                new LifetimeEventsHostedService(serviceProvider, serviceProvider.GetRequiredService<IHostApplicationLifetime>())
            ));
            
            services.AddElasticSearch(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IdentityData identityData)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();
            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            app.UseMiddleware<RequestBufferingMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl = 
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(10)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = 
                    new string[] { "Accept-Encoding" };

                await next();
            });

            // Seeding identity roles
            identityData.Initialize();
            
            app.UsePlatformSwaggerServices();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
