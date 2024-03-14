using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using EventHubConsumer.Consumers;
using Microsoft.Extensions.Logging;

namespace EventHubConsumer.EventHubs;

/// <summary>
/// Encapsulates the functionality for connecting to an Azure Event Hub and managing event consumers.
/// </summary>
/// <param name="connectionString">The connection string for the Azure Event Hub.</param>
/// <param name="consumerGroup">The consumer group to use for reading events from the Event Hub.</param>
/// <param name="eventHubName">The name of the Azure Event Hub.</param>
/// <param name="logger">The logger used for logging events and errors.</param>
/// <param name="consumers">A collection of event handlers responsible for processing events.</param>
/// <typeparam name="TContractEntity">The type of entity that represents the event data.</typeparam>
public class EventHubConsumerProvider<TContractEntity>(
    string connectionString,
    string consumerGroup,
    string eventHubName,
    ILogger logger,
    params IConsumer<TContractEntity>[] consumers
)
{
    private readonly EventHubConsumerClient _consumerClient =
        new(consumerGroup, connectionString, eventHubName);

    // https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/samples/Sample05_ReadingEvents.md#read-events-from-all-partitions
    public async Task ReadMessagesFromAllPartitions(CancellationToken cancellationToken)
    {
        await TryConsumeEventBy(
            cancellationToken,
            async () =>
            {
                using CancellationTokenSource cancellationSource = new CancellationTokenSource();
                await foreach (
                    PartitionEvent partitionEvent in _consumerClient.ReadEventsAsync(
                        cancellationSource.Token
                    )
                )
                {
                    await HandleEvent(partitionEvent);
                }
            }
        );
    }

    // https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/samples/Sample05_ReadingEvents.md#read-events-from-a-partition-starting-from-a-specific-offset
    public async Task ReadMessagesAfterLatestOffset(CancellationToken cancellationToken)
    {
        await TryConsumeEventBy(
            cancellationToken,
            async () =>
            {
                var options = new ReadEventOptions { TrackLastEnqueuedEventProperties = true };
                using CancellationTokenSource cancellationSource = new CancellationTokenSource();
                string firstPartition = (
                    await _consumerClient.GetPartitionIdsAsync(cancellationSource.Token)
                ).First();
                PartitionProperties properties = await _consumerClient.GetPartitionPropertiesAsync(
                    firstPartition,
                    cancellationSource.Token
                );
                EventPosition startingPosition = EventPosition.FromOffset(
                    properties.LastEnqueuedOffset,
                    false
                );
                logger.LogDebug(
                    $"First partitions '{firstPartition}', last partition '{properties.LastEnqueuedOffset}' starting at {startingPosition}"
                );
                await foreach (
                    var partitionEvent in _consumerClient.ReadEventsFromPartitionAsync(
                        firstPartition,
                        startingPosition,
                        options,
                        cancellationSource.Token
                    )
                )
                {
                    await HandleEvent(partitionEvent);
                }
            }
        );
    }

    // TODO: ReadMessagesByDate and this should be complete for all default scenarios

    private async Task HandleEvent(PartitionEvent partitionEvent)
    {
        byte[] eventBodyBytes = partitionEvent.Data.EventBody.ToArray();
        var contractEntity = DeserializeContractEntity(
            eventBodyBytes,
            partitionEvent.Partition.PartitionId
        );
        foreach (var consumer in consumers)
        {
            await consumer.ExecuteAsync(contractEntity!);
        }
    }

    private TContractEntity? DeserializeContractEntity(
        byte[] eventBodyBytes,
        string partitionPartitionId
    )
    {
        var body = Encoding.UTF8.GetString(eventBodyBytes);
        var message = JsonSerializer.Deserialize<TContractEntity>(body);
        logger.LogDebug($"Read event from {partitionPartitionId} value {body}.");
        return message;
    }

    private async Task TryConsumeEventBy(
        CancellationToken cancellationToken,
        Func<Task> asyncAction
    )
    {
        try
        {
            await asyncAction();
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Cancelled externally");
        }
        catch (Exception error)
        {
            logger.LogError(
                "Consumer failed to process event message: {ErrorMessage}",
                error.Message
            );
        }
        finally
        {
            await _consumerClient.CloseAsync(cancellationToken);
        }
    }
}
