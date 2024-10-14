using System.Text;
using RabbitMQ.Client;

namespace RabbitMq.Routing.Core;

public class Sender
{
    private const string ExchangeName = "routing";
    private readonly string[] _routingKeys;

    public Sender(string[] routingKeys)
    {
        _routingKeys = routingKeys.Length > 0 ? routingKeys : new[] {""};
    }

    public void Send(string? message = null)
    {
        message ??= "Example message to sent.";
        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
                Publish(model, message);
            }
        }
    }

    private void Publish(IModel model, string message)
    {
        for (var i = 0; i < _routingKeys.Length; i++)
        {
            var currentMessage = $"[{i.ToString()}][{_routingKeys[i]}] --- {message}";
            var encoded = Encoding.UTF8.GetBytes(currentMessage);
            
            model.BasicPublish(ExchangeName, _routingKeys[i], null, new ReadOnlyMemory<byte>(encoded));
        }
    }
}