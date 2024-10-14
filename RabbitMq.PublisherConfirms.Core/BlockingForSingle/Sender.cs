using System.Text;
using RabbitMQ.Client;

namespace RabbitMq.PublisherConfirms.Core.BlockingForSingle;

public class Sender
{
    private const string ExchangeName = "publisher_confirms";

    public void Send(string? message = null)
    {
        message ??= "Example message to sent.";
        var connectionFactory = new ConnectionFactory() {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
                model.ConfirmSelect();

                byte[]? encoded = Encoding.UTF8.GetBytes(message);
                model.BasicPublish(ExchangeName, "", null, new ReadOnlyMemory<byte>(encoded));
                model.WaitForConfirmsOrDie(new TimeSpan(0 , 0, 15));
            }
        }
    }
}