using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Logs.Serilog;
using Lykke.Middlewares;
using Lykke.SettingsReader;
using Lykke.SettingsReader.ConfigurationProvider;
using Lykke.SettingsReader.SettingsTemplate;
using Lykke.Snow.Common.AssemblyLogging;
using Lykke.Snow.Common.Startup;
using Lykke.Snow.Common.Startup.ApiKey;
using Lykke.Snow.Common.Startup.HttpClientGenerator;
using Lykke.Snow.PriceAlerts.DomainServices.Services;
using Lykke.Snow.PriceAlerts.Extensions;
using Lykke.Snow.PriceAlerts.MappingProfiles;
using Lykke.Snow.PriceAlerts.Modules;
using Lykke.Snow.PriceAlerts.Services;
using Lykke.Snow.PriceAlerts.Settings;
using Lykke.Snow.PriceAlerts.SqlRepositories.MappingProfiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Lykke.Snow.PriceAlerts
{
    internal sealed class Program
    {
        private static readonly string ApiName = "PriceAlerts";

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/service.start.log")
                .CreateBootstrapLogger();

            var restartAttemptsLeft = int.TryParse(Environment.GetEnvironmentVariable("RESTART_ATTEMPTS_NUMBER"),
                out var restartAttemptsFromEnv)
                ? restartAttemptsFromEnv
                : int.MaxValue;
            var restartAttemptsInterval = int.TryParse(
                Environment.GetEnvironmentVariable("RESTART_ATTEMPTS_INTERVAL_MS"),
                out var restartAttemptsIntervalFromEnv)
                ? restartAttemptsIntervalFromEnv
                : 10000;
            while (restartAttemptsLeft > 0)
            {
                try
                {
                    Log.Information("{Name} version {Version}", Assembly.GetEntryAssembly().GetName().Name,
                        Assembly.GetEntryAssembly().GetName().Version.ToString());
                    Log.Information("ENV_INFO: {EnvInfo}", Environment.GetEnvironmentVariable("ENV_INFO"));

                    var builder = WebApplication.CreateBuilder(args);

                    builder.Environment.ContentRootPath =
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    // for serilog configuration and environment variables (e.g. SettingsUrl, etc.)
                    var configurationBuilder = builder.Configuration
                        .SetBasePath(builder.Environment.ContentRootPath)
                        .AddSerilogJson(builder.Environment)
                        .AddEnvironmentVariables();

                    if (Environment.GetEnvironmentVariable("SettingsUrl")?.StartsWith("http") ?? false)
                    {
                        configurationBuilder.AddHttpSourceConfiguration();
                    }

                    var configuration = configurationBuilder.Build();

                    // for the rest of the settings
                    var settingsManager = configuration.LoadSettings<AppSettings>(_ => { });
                    var settings = settingsManager.CurrentValue.PriceAlerts ??
                                   throw new ArgumentException("PriceAlerts settings not found");

                    builder.Services.AddAssemblyLogger();
                    builder.Services.AddSingleton(settings);
                    builder.Services.AddSingleton(settings.Cqrs.ContextNames);
                    builder.Services
                        .AddApplicationInsightsTelemetry()
                        .AddMvcCore()
                        .AddNewtonsoftJson(options =>
                        {
                            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                            options.SerializerSettings.Converters.Add(new StringEnumConverter());
                        })
                        .AddApiExplorer();

                    builder.Services.AddHostedService<PriceAlertsEngine>();

                    builder.Services.AddHealthChecks();

                    builder.Services.AddControllers();

                    builder.Services.AddApiKeyAuth(settings.PriceAlertsClient);

                    builder.Services.AddAuthorization();

                    builder.Services.AddSwaggerGen(options =>
                        {
                            options.SwaggerDoc(
                                "v1",
                                new OpenApiInfo { Version = "v1", Title = $"{ApiName}" });

                            if (!string.IsNullOrWhiteSpace(settings.PriceAlertsClient?.ApiKey))
                            {
                                options.AddApiKeyAwareness();
                            }
                        })
                        .AddSwaggerGenNewtonsoftSupport();

                    builder.Services.AddAutoMapper(typeof(PriceAlertsProfile), typeof(StorageMappingProfile));

                    builder.Services.AddDelegatingHandler(settings.OidcSettings);

                    builder.Services.AddSingleton(provider => new NotSuccessStatusCodeDelegatingHandler());

                    builder.Services.AddMeteorClient();

                    builder.Host
                        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                        .ConfigureContainer<ContainerBuilder>((ctx, cBuilder) =>
                        {
                            // register Autofac modules here
                            cBuilder.RegisterModule(new ServiceModule(settings.MeteorMessageExpiration));
                            if (!ctx.HostingEnvironment.IsEnvironment("test"))
                            {
                                cBuilder.RegisterModule(new RabbitMqModule(settings.RabbitMq));
                                cBuilder.RegisterModule(
                                    new DataModule(settings.Db.ConnectionString));
                                cBuilder.RegisterModule(new CqrsModule(settings.Cqrs));
                                cBuilder.RegisterModule(new ClientsModule(settings));
                            }
                        })
                        .UseSerilog((_, cfg) => cfg.ReadFrom.Configuration(configuration));

                    builder.Services.AddSettingsTemplateGenerator();

                    var app = builder.Build();

                    if (app.Environment.IsDevelopment())
                        app.UseDeveloperExceptionPage();
                    else
                        app.UseHsts();

                    app.UseMiddleware<ExceptionHandlerMiddleware>();

                    app.UseSwagger();
                    app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/v1/swagger.json", ApiName));

                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.MapControllers();
                    app.MapHealthChecks("/healthz");
                    app.MapSettingsTemplate();

                    app.Lifetime.ApplicationStarted.Register(async () =>
                    {
                        var logger = app.Services.GetRequiredService<ILogger<Program>>();
                        try
                        {
                            var startupManager = app.Services.GetRequiredService<StartupManager>();
                            await startupManager.Start();
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Failed to start");
                            app.Lifetime.StopApplication();
                            return;
                        }
                        logger.LogInformation($"{nameof(Program)} started");
                    });

                    await app.RunAsync();
                }
                catch (Exception e)
                {
                    Log.Fatal(e,
                        "Host terminated unexpectedly. Restart in {RestartAttemptsInterval} ms. Attempts left: {RestartAttemptsLeft}",
                        restartAttemptsInterval, restartAttemptsLeft);
                    restartAttemptsLeft--;
                    Thread.Sleep(restartAttemptsInterval);
                }
            }

            // Lets devops to see startup error in console between restarts in the Kubernetes
            var delay = TimeSpan.FromMinutes(1);

            Log.Information("Process will be terminated in {Delay}. Press any key to terminate immediately", delay);

            await Task.WhenAny(
                Task.Delay(delay),
                Task.Run(() => Console.ReadKey(true)));

            Log.Information("Terminated");

            Log.CloseAndFlush();
        }
    }
}