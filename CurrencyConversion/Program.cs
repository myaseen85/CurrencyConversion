
using Asp.Versioning;
using CurrencyConversion.InfraStructureSetup.Swagger;
using CurrencyConversion.Models.Configurations;
using CurrencyConversion.Providers;
using CurrencyConversion.Services.CurrencyExchange;
using CurrencyConversion.Services.Factory;
using CurrencyConversion.Services.Login;
using CurrencyConversion.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Serilog;
using Serilog.Exceptions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.RateLimiting;


namespace CurrencyConversion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Enviroment Settings

            var environment = getEnvironment();
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                                                        .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                                                        .Build();

            Environment.SetEnvironmentVariable(Constant.Constants.AspNetCore_Environment, environment);

            #endregion

            #region SeriLogSettings
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Verbose)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Verbose)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Verbose)
                .WriteTo.File(configurationRoot.GetSection("CurrencyConversionAppSettingConfiguration:LogFile:InfoFilePath").Value, Serilog.Events.LogEventLevel.Information,
                                shared: true,
                                            fileSizeLimitBytes: 304857600,
                                            retainedFileCountLimit: 50,
                                            rollingInterval: RollingInterval.Day,
                                            outputTemplate: "[{Timestamp:HH:mm:ss} {EnvironmentName} {Level:u3} {clientIp} {UserId} {Event} - {Message}{Newline}{Exception}{Newline} {Properties:j}]")
                .WriteTo.File(configurationRoot.GetSection("CurrencyConversionAppSettingConfiguration:LogFile:ErrorFilePath").Value, Serilog.Events.LogEventLevel.Error,
                                shared: true,
                                            fileSizeLimitBytes: 304857600,
                                            retainedFileCountLimit: 50,
                                            rollingInterval: RollingInterval.Day,
                                            outputTemplate: "[{Timestamp:HH:mm:ss} {EnvironmentName} {Level:u3} {ClientIp} {UserId} {Event} - {Message}{Newline}{Exception}{Newline} {Properties:j}]")
                .Enrich.WithExceptionDetails()
                .Enrich.WithClientIp()
                .Enrich.WithCorrelationId()
                .Enrich.WithEnvironmentName().CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            #endregion

            #region Configuration Settings

            builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
            builder.Services.AddConfigurations<CurrencyConversionAppSettingConfiguration>(builder.Configuration, Constant.Constants.CurrencyConversionAppSettingConfiguration);

            #endregion

            #region Swagger


            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlFilePath = Path.Combine(baseDirectory, xmlFileName);
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
            builder.Services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(xmlFilePath);
                c.EnableAnnotations();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
            });

            #endregion

            #region APIs versioning

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }).AddApiExplorer(options =>
               {
                   options.GroupNameFormat = "'v'VVV";
                   options.SubstituteApiVersionInUrl = true;
               });


            #endregion

            #region Fluent Validations

            builder.Services.AddFluentValidationAutoValidation(options =>
              {
                  // Disable the default DataAnnotations validation if you prefer FluentValidation exclusively.
                  options.DisableDataAnnotationsValidation = true;
              }).AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<CurrencyConversionValidator>();

            #endregion

            #region  AddHttpClient
            builder.Services.AddHttpClient("FrankfurterProviderClient")
                    .AddResilienceHandler("RetryPolicy", builder =>
                    {
                        builder.AddRetry(new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = 4,
                            Delay = TimeSpan.FromSeconds(2),
                            BackoffType = DelayBackoffType.Exponential
                        });
                        builder.AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<HttpResponseMessage>
                        {
                            BreakDuration = TimeSpan.FromSeconds(30),
                            FailureRatio = 0.5,

                        });
                        builder.AddTimeout(TimeSpan.FromSeconds(5));
                    });
            #endregion

            #region Rate Limit

            builder.Services.AddRateLimiter(options =>
            {
                // Global rate limiter configuration using IP-based partitions
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    // Use the client's IP address as the partition key
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, key => new FixedWindowRateLimiterOptions
                    {
                        // Allow up to 100 requests per minute per IP address
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        // Optional: allow up to 10 queued requests if limit is reached
                        QueueLimit = 10
                    });
                });
                // Set the status code to return when a client exceeds the limit.
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s). " +
                            $"Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            "Too many requests. Please try again later. " +
                            "Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
                    }
                };
            });

            #endregion

            #region OpenTelemetry

            builder.Services.AddOpenTelemetry()
                            .ConfigureResource(builder => builder.AddService(serviceName: "CurrencyExchangeService"))
                            .WithTracing(builder => builder.AddConsoleExporter())
                            .WithMetrics(builder => builder.AddConsoleExporter());

            #endregion
            // Add services to the container.

            builder.Services.AddScoped<ICurrencyConversionProvider, FrankFurterCurrencyConversionProvider>();
            builder.Services.AddScoped<ICurrencyConversionFactory, CurrencyConversionFactory>();
            builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();
            builder.Services.AddScoped<IUserLogin, UserLogin>();
            builder.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            #region Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configurationRoot.GetSection("CurrencyConversionAppSettingConfiguration:JwtSetttings:Issuer").Value,
                    ValidAudience = configurationRoot.GetSection("CurrencyConversionAppSettingConfiguration:JwtSetttings:Issuer").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationRoot.GetSection("CurrencyConversionAppSettingConfiguration:JwtSetttings:Key").Value))
                };
            });

            #endregion

            var app = builder.Build();

            app.UseMiddleware<RequestLoggingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
        public static string getEnvironment()
        {
            var baseConfiguration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            return baseConfiguration.GetSection("Environment").Value;
        }

    }

    public static class ConfigurationExtension
    {
        public static void AddConfigurations<T>(this IServiceCollection services, IConfiguration configuration, string? confgurationTag = null) where T : class
        {
            var instance = Activator.CreateInstance<T>();
            new ConfigureFromConfigurationOptions<T>(configuration.GetSection(confgurationTag)).Configure(instance);
            services.AddSingleton(instance);
        }
    }


}
