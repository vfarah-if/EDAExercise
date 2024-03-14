using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Company.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GettingStarted
{
    public enum BusTypeDemoOption
    {
        InMemory = 0,
        ServiceBus = 1,
        EventHub = 2,
    }

    public class Program
    {
        private static readonly BusTypeDemoOption _busTypeDemoOption = BusTypeDemoOption.InMemory;
        private static IConfiguration _configuration;
        private static ILogger _logger;

        public static async Task Main(string[] args)
        {
            ConfigureAppSettings();
            ConfigureLogging();
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static void ConfigureAppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
        }

        private static void ConfigureLogging()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConfiguration(_configuration.GetSection("Logging")).AddConsole();
            });
            _logger = loggerFactory.CreateLogger<Program>();
            _logger.LogDebug("Ready for logging");
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddMassTransit(busRegistrationConfigurator =>
                        {
                            busRegistrationConfigurator.SetKebabCaseEndpointNameFormatter();
                            busRegistrationConfigurator.SetInMemorySagaRepositoryProvider();

                            var entryAssembly = Assembly.GetEntryAssembly();

                            busRegistrationConfigurator.AddConsumers(entryAssembly);
                            busRegistrationConfigurator.AddSagaStateMachines(entryAssembly);
                            busRegistrationConfigurator.AddSagas(entryAssembly);
                            busRegistrationConfigurator.AddActivities(entryAssembly);
                            switch (_busTypeDemoOption)
                            {
                                case BusTypeDemoOption.EventHub:
                                    var sbConnectionString = _configuration.GetConnectionString(
                                        "ServiceBus"
                                    );
                                    var eventHubConnectionString =
                                        _configuration.GetConnectionString("EventHub");
                                    if (
                                        eventHubConnectionString != null
                                        && sbConnectionString != null
                                    )
                                    {
                                        _logger.LogInformation($"Azure Event Hub demo");
                                        busRegistrationConfigurator.UsingAzureServiceBus(
                                            (context, cfg) =>
                                            {
                                                cfg.Host(sbConnectionString);

                                                cfg.ConfigureEndpoints(context);
                                            }
                                        );

                                        busRegistrationConfigurator.AddRider(rider =>
                                        {
                                            rider.AddConsumer<HelloMessageConsumer>();
                                            rider.UsingEventHub(
                                                (context, k) =>
                                                {
                                                    k.Host(eventHubConnectionString);
                                                    k.Storage(eventHubConnectionString);
                                                    k.ReceiveEndpoint(
                                                        "hello-message",
                                                        c =>
                                                        {
                                                            c.ConfigureConsumer<HelloMessageConsumer>(
                                                                context
                                                            );
                                                        }
                                                    );
                                                }
                                            );
                                        });
                                    }
                                    break;
                                case BusTypeDemoOption.ServiceBus:
                                    var connectionString = _configuration.GetConnectionString(
                                        "ServiceBus"
                                    );
                                    if (connectionString != null)
                                    {
                                        _logger.LogInformation(
                                            "Configuring Azure Service Bus demo"
                                        );
                                        busRegistrationConfigurator.UsingAzureServiceBus(
                                            (context, configuration) =>
                                            {
                                                configuration.Host(connectionString);
                                                configuration.ConfigureEndpoints(context);
                                            }
                                        );
                                    }
                                    break;
                                case BusTypeDemoOption.InMemory:
                                default:
                                    _logger.LogInformation("Configuring memory demo");
                                    busRegistrationConfigurator.UsingInMemory(
                                        (context, cfg) =>
                                        {
                                            cfg.ConfigureEndpoints(context);
                                        }
                                    );
                                    break;
                            }
                        });

                        services.AddHostedService<SimplePublishService>();
                    }
                );
    }
}
