using MassTransit;

namespace ConsumersWebApp.Messaging;

public class SayHelloConsumer(ILogger<SayHelloConsumer> logger) : IConsumer<SendNameMessage>
{
    public Task Consume(ConsumeContext<SendNameMessage> context)
    {
        if (context.Message.Name == "Error") 
            throw new ArgumentNullException(nameof(context.Message.Name));
        logger.LogInformation("Hello {Name}!", context.Message.Name);
        return Task.CompletedTask;
    }
}