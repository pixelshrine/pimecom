var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new
{
    Service = "MessageQueue",
    Description = "RabbitMQ is expected to run as infrastructure for product-events."
}));

app.Run();
