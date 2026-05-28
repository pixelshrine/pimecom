using Microsoft.EntityFrameworkCore;
using Pim.Application.Messaging;
using Pim.Application.Repositories;
using Pim.Application.Services;
using Pim.Infrastructure.Data;
using Pim.Infrastructure.Messaging;
using Pim.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Configuration.GetValue("UseInMemoryDatabase", true))
    {
        options.UseInMemoryDatabase("PimDb");
        return;
    }

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IEventBus, RabbitMqBus>();

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
