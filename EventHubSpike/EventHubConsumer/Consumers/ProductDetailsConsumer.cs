using Domain.Contracts;
using Domain.Extensions;
using Microsoft.Extensions.Logging;

namespace EventHubConsumer.Consumers;

public class ProductDetailConsumer(ILogger logger) : IConsumer<ProductDetail>
{
    public Task ExecuteAsync(ProductDetail data)
    {
        try
        {
            if (data.IsValid(out var errors))
            {
                logger.LogInformation(
                    $"Example Product DetailMessage Handler executed for '{data.SerialNumber}'"
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
