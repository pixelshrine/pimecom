using Ecom.Application.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messaging;
using System.Text;
using System.Text.Json;

namespace Ecom.Application.Infrastructure.Messaging;

public class RabbitMqConsumer : BackgroundService
{
    private readonly EventRouter _router;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IConfiguration _configuration;

    public RabbitMqConsumer(
        EventRouter router,
        ILogger<RabbitMqConsumer> logger,
        IConfiguration configuration)
    {
        _router = router;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["Messaging:RabbitMqHost"] ?? "localhost",
                    Port = int.TryParse(_configuration["Messaging:RabbitMqPort"], out var mqport) ? mqport : 5672,
                    UserName = "guest",
                    Password = "guest"
                };
                var queueName = _configuration["Messaging:ProductEventsQueue"] ?? "product-events";

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(
                            ea.Body.ToArray());

                        var envelope = JsonSerializer
                            .Deserialize<EventEnvelope>(json);

                        if (envelope == null)
                        {
                            channel.BasicAck(
                                ea.DeliveryTag,
                                false);
                            return;
                        }

                        await _router.Route(envelope);

                        channel.BasicAck(
                            ea.DeliveryTag,
                            false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "RabbitMQ consumer failed");

                        channel.BasicReject(
                            ea.DeliveryTag,
                            false);
                    }
                };

                channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Listening for RabbitMQ product events.");

                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ is unavailable. Retrying in 10 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
