using System;
using System.Net;
using System.Text;
using AutoMapper;
using Delivery.Api.Auth;
using Delivery.Api.CommandHandler;
using Delivery.Api.Domain.Command;
using Delivery.Api.Domain.Query;
using Delivery.Api.Extensions;
using Delivery.Api.Helpers;
using Delivery.Api.Models;
using Delivery.Api.Models.Dto;
using Delivery.Api.QueryHandler;
using Delivery.Api.Utils.Configs;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Product.Domain.CommandHandlers;
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
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

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
            });

            // api user claim policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
            });

            // add identity
            var builder = services.AddIdentityCore<ApplicationUser>(o =>
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

            services.AddControllers();

            services.AddHttpClient();

            //register handlers
            services.AddScoped<ICommandHandler<CreateOrderCommand, bool>, OrderCommandHandler>();
            services.AddScoped<ICommandHandler<CreateReportOrderCommand, bool>, ReportOrderCommandHandler>();
            services.AddScoped<ICommandHandler<CreateProductCommand, bool>, CreateProductCommandHandler>();

            services.AddScoped<IQueryHandler<GetOrderByCustomerIdQuery, OrderViewDto[]>, OrdersByCustomerIdQueryHandler>();
            services.AddScoped<IQueryHandler<CategoryByIdQuery, CategoryDto>, CategoryByIdQueryHandler>();
            services.AddScoped<IQueryHandler<ProductGetAllQuery, ProductDto[]>, ProductGetAllQueryHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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


            app.UseExceptionHandler(
             builder =>
             {
                 builder.Run(
                            async context =>
                         {
                             context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                             context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                             var error = context.Features.Get<IExceptionHandlerFeature>();
                             if (error != null)
                             {
                                 context.Response.AddApplicationError(error.Error.Message);
                                 await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                             }
                         });
             });

            app.UseAuthentication();
            //app.UseIdentityServer();
            app.UseAuthorization();

            // use middelware response cache
            app.UseResponseCaching();
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
