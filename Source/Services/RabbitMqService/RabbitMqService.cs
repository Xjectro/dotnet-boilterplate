using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Source.Configurations;
using System.Text;

namespace Source.Services.RabbitMqService;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private readonly object _lock = new object();

    public RabbitMqService(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;
    }

    public IConnection GetConnection()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            lock (_lock)
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _settings.Host,
                        Port = _settings.Port,
                        UserName = _settings.Username,
                        Password = _settings.Password,
                        VirtualHost = _settings.VirtualHost,
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                    };

                    _connection = factory.CreateConnection();
                }
            }
        }

        return _connection;
    }

    public IModel CreateChannel()
    {
        var connection = GetConnection();
        return connection.CreateModel();
    }

    public void PublishMessage(string queueName, string message)
    {
        using var channel = CreateChannel();
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(message);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
    }

    public void PublishMessage(string exchangeName, string routingKey, string message)
    {
        using var channel = CreateChannel();

        var body = Encoding.UTF8.GetBytes(message);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);
    }

    public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
    {
        using var channel = CreateChannel();
        channel.QueueDeclare(queue: queueName, durable: durable, exclusive: exclusive, autoDelete: autoDelete, arguments: null);
    }

    public void DeclareExchange(string exchangeName, string type = ExchangeType.Direct, bool durable = true, bool autoDelete = false)
    {
        using var channel = CreateChannel();
        channel.ExchangeDeclare(exchange: exchangeName, type: type, durable: durable, autoDelete: autoDelete);
    }

    public void BindQueue(string queueName, string exchangeName, string routingKey)
    {
        using var channel = CreateChannel();
        channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
