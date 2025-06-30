using System.Text;
using System.Text.Json;
using GamifyApi.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQListenerService : BackgroundService
{
    private readonly ILogger<RabbitMQListenerService> _logger;
    public readonly string _queueName = "order";
    public readonly int port = 5672;
    ConnectionFactory factory = new ConnectionFactory()
    {
        HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "localhost",
        Port = 5672
    };

    public RabbitMQListenerService(ILogger<RabbitMQListenerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false,
        arguments: null);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");
            if (message != null)
            {
                OrderRequest result = JsonSerializer.Deserialize<OrderRequest>(message);
                Console.WriteLine("Serialized Successfully");
            }
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync("order", autoAck: true, consumer: consumer);

        // Keep running until cancelled
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}