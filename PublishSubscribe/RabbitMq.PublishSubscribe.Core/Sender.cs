using System.Text;
using RabbitMQ.Client;

namespace RabbitMq.PublishSubscribe.Core;

public class Sender
{
    private const string ExchangeName = "publish_subscribe";

    public void Send(string? message = null)
    {
        message ??= "Example message to sent.";
        var connectionFactory = new ConnectionFactory() {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);

                byte[]? encoded = Encoding.UTF8.GetBytes(message);
                model.BasicPublish(ExchangeName, "", null, new ReadOnlyMemory<byte>(encoded));
            }
        }
    }
}