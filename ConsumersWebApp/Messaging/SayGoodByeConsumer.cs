using MassTransit;

namespace ConsumersWebApp.Messaging;

public class SayGoodByeConsumer(ILogger<SayGoodByeConsumer> logger) : IConsumer<SendNameMessage>
{
    public Task Consume(ConsumeContext<SendNameMessage> context)
    {
        logger.LogInformation("Goodbye {Name}!", context.Message.Name);
        return Task.CompletedTask;
    }
}