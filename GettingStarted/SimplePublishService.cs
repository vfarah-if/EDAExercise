using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace GettingStarted
{
    public class SimplePublishService(IBus bus) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PublishHelloMessage(stoppingToken);
                // await SendHelloMessage(stoppingToken);
                await Task.Delay(2000, stoppingToken);
            }
        }

        // Fans out to everyone
        private async Task PublishHelloMessage(CancellationToken stoppingToken)
        {
            // Doesn't care about the endpoint and publish to all things listening
            await bus.Publish(CreateMessage(), stoppingToken);
        }

        // Cares about the end-point specifically
        private async Task SendHelloMessage(CancellationToken stoppingToken)
        {
            var endpoint = await bus.GetSendEndpoint(new Uri("loopback://localhost/hello-message"));
            await endpoint.Send(CreateMessage(), stoppingToken);
        }

        private static HelloMessage CreateMessage()
        {
            return new HelloMessage
            {
                CommandId = InVar.Id,
                // CommandId = NewId.NextGuid(),
                // CommandId = Guid.NewGuid(),
                Name = "World",
                Created = InVar.Timestamp,
            };
        }
    }
}
