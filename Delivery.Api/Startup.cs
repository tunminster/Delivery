using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.Utils.Configs;
using Delivery.Azure.Library.Configuration.Environments;
using Delivery.Azure.Library.Configuration.Environments.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Stdout;
using Delivery.Azure.Library.WebApi.Middleware;
using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.QueryHandlers;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Extensions;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.QueryHandlers;
using Delivery.Product.Domain.CommandHandlers;
using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.QueryHandlers;
using Delivery.Azure.Library.Configuration;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Features;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Delivery.Azure.Library.KeyVault.Providers;
using Delivery.Azure.Library.Resiliency.Stability;
using Delivery.Azure.Library.Resiliency.Stability.Interfaces;
using Delivery.Azure.Library.Storage.Cosmos.Connections;
using Delivery.Azure.Library.Storage.Cosmos.Interfaces;
using Delivery.Azure.Library.WebApi.Filters;
using Delivery.Database.Models;
using Delivery.Database.Seeding;
using Delivery.Domain.Factories.Auth;
using Delivery.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
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
            
            services.AddIdentity<Database.Models.ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
            

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
                            x.User.HasClaim(ClaimTypes.Role, ClaimData.JwtClaimIdentifyClaim.ClaimValue)));
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

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Delivery Api", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
            });

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
            //app.UseIdentityServer();
            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            app.UseMiddleware<RequestBufferingMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            

            app.UseAuthentication();
            app.UseAuthorization();

            // use middelware response cache
            //app.UseResponseCaching();
            
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
            
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
