namespace Company.Consumers
{
    using MassTransit;

    public class HelloMessageConsumerDefinition : ConsumerDefinition<HelloMessageConsumer>
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<HelloMessageConsumer> consumerConfigurator
        )
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
        }
    }
}
