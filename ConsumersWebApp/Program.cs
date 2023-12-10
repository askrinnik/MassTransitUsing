using ConsumersWebApp.Messaging;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

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

// Export metrics from all HTTP clients registered in services
//builder.Services.UseHttpClientMetrics(); // add the prometheus-net.AspNetCore nuget package

Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
{
    // Labels applied to all metrics in the registry.
    { "env", "testingEnv" },
    { "domain", "myDomain" },
    { "app", "MyApp" },
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
//            .AddOtlpExporter() // port 4317 // add the OpenTelemetry.Exporter.OpenTelemetryProtocol nuget package
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRouting();
// Capture metrics about all received HTTP requests.
//app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();
app.UseEndpoints(endpoints =>
{
    // Enable the /metrics page to export Prometheus metrics.
    // Open http://localhost:5099/metrics to see the metrics.
    //
    // Metrics published in this sample:
    // * built-in process metrics giving basic information about the .NET runtime (enabled by default)
    // * metrics from .NET Event Counters (enabled by default, updated every 10 seconds)
    // * metrics from .NET Meters (enabled by default)
    // * metrics about requests made by registered HTTP clients used in SampleService (configured above)
    // * metrics about requests handled by the web app (configured above)
    // * ASP.NET health check statuses (configured above)
    // * custom business logic metrics published by the SampleService class
    endpoints.MapMetrics();
});


app.Run();