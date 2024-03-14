using System.Reflection;
using Microsoft.Extensions.Logging;

namespace EventHubConsumer.Consumers;

public class ConsumerDiscovery
{
    public static IEnumerable<IConsumer<T>?> DiscoverConsumers<T>(ILogger logger)
    {
        var consumerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IConsumer<T>).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

        foreach (var consumerType in consumerTypes)
        {
            yield return (IConsumer<T>?)Activator.CreateInstance(consumerType, logger);
        }
    }
}