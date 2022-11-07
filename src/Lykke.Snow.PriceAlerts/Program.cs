using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Logs.Serilog;
using Lykke.Middlewares;
using Lykke.SettingsReader;
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
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Lykke.Snow.PriceAlerts
{
    internal sealed class Program
    {
        private static readonly string ApiName = "PriceAlerts";
        
        private static readonly List<(string, string, string)> EnvironmentSecretConfig = new List<(string, string, string)>
        {
            /* secrets.json Key                               // Environment Variable               // default value (optional) */
            ("Api-Authority",                    "API_AUTHORITY",                      null),
            ("Client-Id",                        "CLIENT_ID",                          null),
            ("Client-Secret",                    "CLIENT_SECRET",                      null),
            ("Client-Scope",                     "CLIENT_SCOPE",                       null),
            ("Validate-Issuer-Name",             "VALIDATE_ISSUER_NAME",               null),
            ("Require-Https",                    "REQUIRE_HTTPS",                      null),
            ("Renew-Token-Timeout-Sec",          "RENEW_TOKEN_TIMEOUT_SEC",            null),
        };

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/service.start.log")
                .CreateBootstrapLogger();

            try
            {
                Log.Information("{Name} version {Version}", Assembly.GetEntryAssembly().GetName().Name,
                    Assembly.GetEntryAssembly().GetName().Version.ToString());
                Log.Information("ENV_INFO: {EnvInfo}", Environment.GetEnvironmentVariable("ENV_INFO"));

                var builder = WebApplication.CreateBuilder(args);

                builder.Environment.ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                var configuration = builder.Configuration
                    .SetBasePath(builder.Environment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true)
                    .AddSerilogJson(builder.Environment)
                    .AddUserSecrets<Program>()
                    .AddEnvironmentVariables()
                    .AddEnvironmentSecrets<Program>(EnvironmentSecretConfig)
                    .Build();

                // Lykke settings manager for using settings service
                var settingsManager = configuration.LoadSettings<AppSettings>(_ => { });

                builder.Services.AddSingleton(settingsManager.CurrentValue);
                builder.Services.AddSingleton(settingsManager.CurrentValue.PriceAlerts.Cqrs.ContextNames);
                // Add services to the container.
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

                builder.Services.AddApiKeyAuth(settingsManager.CurrentValue.PriceAlerts?.PriceAlertsClient);

                builder.Services.AddAuthorization();

                builder.Services.AddSwaggerGen(options =>
                    {
                        options.SwaggerDoc(
                            "v1",
                            new OpenApiInfo {Version = "v1", Title = $"{ApiName}"});

                        if (!string.IsNullOrWhiteSpace(settingsManager.CurrentValue.PriceAlerts.PriceAlertsClient?.ApiKey))
                        {
                            options.AddApiKeyAwareness();
                        }
                    })
                    .AddSwaggerGenNewtonsoftSupport();

                builder.Services.AddAutoMapper(typeof(PriceAlertsProfile), typeof(StorageMappingProfile));

                var settings = settingsManager.CurrentValue.PriceAlerts;
                builder.Services.AddDelegatingHandler(configuration);
                
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
                            cBuilder.RegisterModule(
                                new DataModule(settingsManager.CurrentValue.PriceAlerts.Db.ConnectionString));
                            cBuilder.RegisterModule(new CqrsModule(settingsManager.CurrentValue.PriceAlerts.Cqrs));
                            cBuilder.RegisterModule(new ClientsModule(settingsManager.CurrentValue));
                        }
                    })
                    .UseSerilog((_, cfg) => cfg.ReadFrom.Configuration(configuration));

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

                var startupManager = app.Services.GetRequiredService<StartupManager>();
                await startupManager.Start();
                await app.RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Log.Information("Process will be terminated in {Delay}. Press any key to terminate immediately", delay);

                await Task.WhenAny(
                    Task.Delay(delay),
                    Task.Run(() => Console.ReadKey(true)));

                Log.Information("Terminated");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}