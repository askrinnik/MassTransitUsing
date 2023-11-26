using System.Globalization;
using ConsumersWebApp.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace ConsumersWebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProducerController(
    IRequestClient<GetTimeRequest> requestClient,
    IPublishEndpoint publishEndpoint) 
    : ControllerBase
{
    [HttpGet("GetTime/{city}")]
    public async Task<string> GetTimeForCity(string city)
    {
        var response = await requestClient.GetResponse<GetTimeResponse>(new(city));
        return $"Time in {city}: {response.Message.CurrentTime.ToString(CultureInfo.InvariantCulture)}";
    }

    [HttpPost("SendName/{name}")]
    public async Task Post(string name)
    {
        await publishEndpoint.Publish<SendNameMessage>(new(name));
    }
}