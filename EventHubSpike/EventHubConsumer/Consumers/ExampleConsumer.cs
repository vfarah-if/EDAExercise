using Domain.Contracts;
using Domain.Extensions;
using Microsoft.Extensions.Logging;

namespace EventHubConsumer.Consumers;

public class ExampleConsumer(ILogger logger) : IConsumer<TestMessage>
{
    public Task ExecuteAsync(TestMessage data)
    {
        try
        {
            if (data.IsValid(out var errors))
            {
                logger.LogInformation(
                    $"Example Text Message Handler executed for '{data.Identifier}'"
                );
                return Task.CompletedTask;
            }
            logger.LogWarning("Data is dodge dude!");
            return Task.FromResult(errors);
        }
        catch (Exception error)
        {
            logger.LogError("Failed to process event message: {ErrorMessage}", error.Message);
            return Task.FromException(error);
        }
    }
}
