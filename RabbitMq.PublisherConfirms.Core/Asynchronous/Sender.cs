using System.Text;
using RabbitMQ.Client;

namespace RabbitMq.PublisherConfirms.Core.Asynchronous;

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
                model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
                model.ConfirmSelect();

                byte[]? encoded = Encoding.UTF8.GetBytes(message);
                model.BasicPublish(ExchangeName, "", null, new ReadOnlyMemory<byte>(encoded));
                model.BasicAcks += (sender, args) =>
                {
                    Console.WriteLine("Received");
                };
                model.BasicNacks += (sender, args) =>
                {
                    Console.WriteLine("Rejected");
                };
            }
        }
    }
}