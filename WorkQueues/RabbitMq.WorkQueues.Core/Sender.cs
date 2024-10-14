using System.Text;
using RabbitMQ.Client;

namespace RabbitMq.WorkQueues.Core;

public class Sender
{
    private const string QueueName = "work_queues";

    public void Send(string? message = null)
    {
        message ??= "Example message to sent.";
        var connectionFactory = new ConnectionFactory() {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.QueueDeclare(QueueName, true, false, false, null);
                
                byte[]? encoded = Encoding.UTF8.GetBytes(message);
                model.BasicPublish("", QueueName, null, new ReadOnlyMemory<byte>(encoded));
            }
        }
    }
}