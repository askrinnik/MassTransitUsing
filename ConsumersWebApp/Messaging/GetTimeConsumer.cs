using MassTransit;

namespace ConsumersWebApp.Messaging;

public class GetTimeConsumer(ILogger<GetTimeConsumer> logger)
    : IConsumer<GetTimeRequest>
{
    public async Task Consume(ConsumeContext<GetTimeRequest> context)
    {
        logger.LogInformation($"Received {nameof(GetTimeRequest)} with city {context.Message.City}");
        await context.RespondAsync<GetTimeResponse>(new(TimeOnly.FromDateTime(DateTime.Now)));
    }
}