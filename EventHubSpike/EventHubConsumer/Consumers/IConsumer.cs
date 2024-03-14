namespace EventHubConsumer.Consumers;

public interface IConsumer<in T>
{
    Task ExecuteAsync(T data);
}
