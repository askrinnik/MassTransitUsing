using ConsumersWebApp.Messaging;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x => 
{
    x.AddConsumer<GetTimeConsumer>();
    x.AddConsumer<SayHelloConsumer>();
    x.AddConsumer<SayGoodByeConsumer>();
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
        resourceBuilder.AddService("MassTransitUsing", serviceInstanceId: Environment.MachineName))
    .WithTracing(tracerProviderBuilder => tracerProviderBuilder
            .AddSource(DiagnosticHeaders.DefaultListenerName)
            .AddAspNetCoreInstrumentation( // add the pre-release OpenTelemetry.Instrumentation.AspNetCore nuget package
                options =>
                {
                    options.RecordException = true;
                    options.Filter = httpContext =>
                    {
                        var pathValue = httpContext.Request.Path.Value;
                        var swaggerUrls = new[] { "/swagger", "/_vs", "/_framework" };
                        return pathValue is null || swaggerUrls.All(url => !pathValue.StartsWith(url));
                    };
                })
            .AddOtlpExporter() // port 4317 // add the OpenTelemetry.Exporter.OpenTelemetryProtocol nuget package
    )
    .WithMetrics(b => b
            .AddMeter(InstrumentationOptions.MeterName) // MassTransit Meter
            .AddOtlpExporter() // port 4317 // add the OpenTelemetry.Exporter.OpenTelemetryProtocol nuget package
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();