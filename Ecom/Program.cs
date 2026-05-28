using Ecom.Application.Infrastructure.Data;
using Ecom.Application.Infrastructure.Messaging;
using Ecom.Application.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Configuration.GetValue("UseInMemoryDatabase", true))
    {
        options.UseInMemoryDatabase("EcomDb");
        return;
    }

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddSingleton<EventRouter>();

builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("all", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

await DatabaseInitializer.EnsureDatabaseCreatedAsync(
    app.Services,
    app.Logger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("all");

app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//    db.Database.Migrate();
//}

app.Run();
