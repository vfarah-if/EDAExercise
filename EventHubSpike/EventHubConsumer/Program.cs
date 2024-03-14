using Azure.Messaging.EventHubs.Consumer;
using Domain.Contracts;
using EventHubConsumer.Consumers;
using EventHubConsumer.EventHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventHubConsumer
{
    class Program
    {
        private static IConfiguration _configuration = null!;
        private static ILogger _logger = null!;
        private static bool withReload;
        private static EventHubConsumerProvider<TestMessage> _testMessageConsumerProvider = null!;
        private static string? _connectionString;
        private static string? _eventHubName;

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            ConfigureAppSettings();
            ConfigureLogging();
            ParseArguments(args);
            ConfigureEventHubSettings();
            _logger.LogInformation("Generating consumer demo ...");
            if (!string.IsNullOrEmpty(_connectionString) && !string.IsNullOrEmpty(_eventHubName))
            {
                /*
                 * Utilise dependency injection or reflection to get consumers by type
                 */
                var consumers = ConsumerDiscovery
                    .DiscoverConsumers<TestMessage>(_logger)
                    .ToArray();
                _testMessageConsumerProvider = new EventHubConsumerProvider<TestMessage>(
                    _connectionString,
                    EventHubConsumerClient.DefaultConsumerGroupName,
                    _eventHubName,
                    _logger,
                    consumers!  // or new ExampleConsumer(_logger), new ExampleOtherConsumer(_logger)
            );
                using var cancellationSource = new CancellationTokenSource();
                await (
                    withReload
                        ? _testMessageConsumerProvider.ReadMessagesFromAllPartitions(
                            cancellationSource.Token
                        )
                        : _testMessageConsumerProvider.ReadMessagesAfterLatestOffset(
                            cancellationSource.Token
                        )
                );
            }
            else
            {
                _logger.LogWarning("App settings have not been configured as expected");
            }
        }

        private static void ConfigureEventHubSettings()
        {
            var eventHubSettingsSection = _configuration.GetSection("EventHubSettings");
            _connectionString = eventHubSettingsSection
                .GetSection("ConnectionStrings")
                .GetSection("DefaultConnection")
                .Value;
            _eventHubName = eventHubSettingsSection.GetSection("Name").Value;
        }

        private static void ParseArguments(string[] args)
        {
            foreach (string arg in args)
            {
                var parts = arg.Split('=');

                if (parts.Length == 2)
                {
                    var option = parts[0].ToLower().Trim();
                    var value = parts[1].ToLower().Trim();

                    switch (option)
                    {
                        case "--with-reload":
                            bool.TryParse(value, out withReload);
                            break;
                        default:
                            withReload = false;
                            _logger.LogWarning($"No load option configured: {option}");
                            break;
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid argument format: {arg}");
                }
            }
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

        private static void ConfigureAppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            _configuration = builder.Build();
        }
    }
}
