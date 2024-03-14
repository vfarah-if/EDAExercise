using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Domain.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventHubProducer
{
    class Program
    {
        private static IConfiguration _configuration = null!;
        private static ILogger _logger = null!;
        private static EventHubBufferedProducerClient _producer = null!;

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            ConfigureAppSettings();
            ConfigureLogging();
            var eventHubSettingsSection = _configuration.GetSection("EventHubSettings");
            var connectionString = eventHubSettingsSection
                .GetSection("ConnectionStrings")
                .GetSection("DefaultConnection")
                .Value;
            var eventHubName = eventHubSettingsSection.GetSection("Name").Value;

            if (connectionString != null && !string.IsNullOrEmpty(eventHubName))
            {
                _logger.LogInformation($"Generating producer spike for {eventHubName}");
                ConfigureProducer(connectionString, eventHubName);
                await SendMessagesToEventHub(10);
            }
        }

        // Create a producer based on https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/samples
        private static void ConfigureProducer(string connectionString, string eventHubName)
        {
            _producer = new EventHubBufferedProducerClient(connectionString, eventHubName);
            _producer.SendEventBatchFailedAsync += eventArgs =>
            {
                _logger.LogWarning($"Publishing failed for {eventArgs.EventBatch.Count}");
                return Task.CompletedTask;
            };
            _producer.SendEventBatchSucceededAsync += eventArgs =>
            {
                _logger.LogInformation(
                    $"{eventArgs.EventBatch.Count} events were published to partition: '{eventArgs.PartitionId}'."
                );
                return Task.CompletedTask;
            };
        }

        private static async Task SendMessagesToEventHub(int messageCount)
        {
            try
            {
                for (var index = 0; index < messageCount; ++index)
                {
                    var testMessage = new TestMessage
                    {
                        CommandId = Guid.NewGuid(),
                        Identifier = $"Event demo {index}"
                    };
                    var json = JsonSerializer.Serialize(testMessage);
                    var eventData = new EventData(json);
                    await _producer.EnqueueEventAsync(eventData);
                }
            }
            finally
            {
                await _producer.CloseAsync();
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
