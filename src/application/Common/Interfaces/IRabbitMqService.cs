using RabbitMQ.Client;

namespace Api.Application.Common.Interfaces;

public interface IRabbitMqService
{
    IConnection GetConnection();
    IModel CreateChannel();
    void PublishMessage(string queueName, string message);
    void PublishMessage(string exchangeName, string routingKey, string message);
    void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
    void DeclareExchange(string exchangeName, string type = ExchangeType.Direct, bool durable = true, bool autoDelete = false);
    void BindQueue(string queueName, string exchangeName, string routingKey);
}
