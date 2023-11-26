using MassTransit;

namespace ConsumersWebApp.Messaging;

public class SayHelloConsumer(ILogger<SayHelloConsumer> logger) : IConsumer<SendNameMessage>
{
    public Task Consume(ConsumeContext<SendNameMessage> context)
    {
        logger.LogInformation("Hello {Name}!", context.Message.Name);
        return Task.CompletedTask;
    }
}