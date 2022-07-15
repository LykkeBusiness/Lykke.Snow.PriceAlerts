using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Logs.Serilog;
using Lykke.Middlewares;
using Lykke.SettingsReader;
using Lykke.Snow.PriceAlerts.Modules;
using Lykke.Snow.PriceAlerts.Settings;
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
        private static string ApiName = "PriceAlerts";

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
                    .Build();

                // Lykke settings manager for using settings service
                var settingsManager = configuration.LoadSettings<AppSettings>(_ => { });

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

                builder.Services.AddHealthChecks();
                
                builder.Services.AddControllers();

                builder.Services.AddSwaggerGen(options =>
                    {
                        options.SwaggerDoc(
                            "v1",
                            new OpenApiInfo { Version = "v1", Title = $"{ApiName}" });

                        // Add api key awareness if required
                    })
                    .AddSwaggerGenNewtonsoftSupport();

                builder.Host
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureContainer<ContainerBuilder>((ctx, cBuilder) =>
                    {
                        // register Autofac modules here
                        cBuilder.RegisterModule(new ServiceModule());
                    })
                    .UseSerilog((_, cfg) => cfg.ReadFrom.Configuration(configuration));

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }

                app.UseMiddleware<ExceptionHandlerMiddleware>();

                app.UseSwagger();
                app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/v1/swagger.json", ApiName));

                app.MapControllers();
                app.MapHealthChecks("/healthz");

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