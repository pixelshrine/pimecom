using Pim.Application.Messaging;
using RabbitMQ.Client;
using Shared.Messaging;
using System.Text;
using System.Text.Json;

namespace Pim.Infrastructure.Messaging;

public class RabbitMqBus : IEventBus
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqBus> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public RabbitMqBus(
        IConfiguration configuration,
        ILogger<RabbitMqBus> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Publish(EventEnvelope evt)
    {
        if (!_configuration.GetValue("Messaging:Enabled", true))
        {
            _logger.LogInformation("Messaging is disabled. Skipping event {EventType}.", evt.EventType);
            return;
        }

        var queueName = _configuration["Messaging:ProductEventsQueue"] ?? "product-events";
        var factory = new ConnectionFactory
        {
            HostName = _configuration["Messaging:RabbitMqHost"] ?? "localhost",
            Port = int.TryParse(_configuration["Messaging:RabbitMqPort"], out var mqport) ? mqport : 5672,
            UserName = "guest",
            Password = "guest"
        };

        try
        {
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(evt));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);
        }
        catch (Exception ex) when (_configuration.GetValue("Messaging:IgnorePublishFailures", true))
        {
            _logger.LogWarning(ex, "RabbitMQ is unavailable. Event {EventType} was not published.", evt.EventType);
            await TryPostToDevelopmentFallback(evt);
        }
    }

    private async Task TryPostToDevelopmentFallback(EventEnvelope evt)
    {
        var endpoint = _configuration["Messaging:DevelopmentFallbackEndpoint"];

        if (string.IsNullOrWhiteSpace(endpoint))
            return;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(endpoint, evt);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Development event fallback returned status code {StatusCode}.",
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Development event fallback failed.");
        }
    }
}
