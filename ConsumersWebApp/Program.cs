using ConsumersWebApp.Messaging;
using MassTransit;

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