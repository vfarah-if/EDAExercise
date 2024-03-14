using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Company.Consumers
{
    public class HelloMessageConsumer(ILogger<HelloMessageConsumer> logger)
        : IConsumer<HelloMessage>
    {
        public Task Consume(ConsumeContext<HelloMessage> context)
        {
            logger.LogInformation(
                message: $"Hello {context.Message.Name} on {context.Message.Created:F} {context.Message.CommandId} with correlation {context.CorrelationId}"
            );
            return Task.CompletedTask;
        }
    }
}
