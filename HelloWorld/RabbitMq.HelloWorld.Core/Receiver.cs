using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.HelloWorld.Core;

public class Receiver
{
    private const string QueueName = "hello_world";

    public void Run()
    {
        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.QueueDeclare(QueueName, false, false, false, null);

                var customer = new EventingBasicConsumer(model);
                customer.Received += CustomerOnReceived;
                
                model.BasicConsume(QueueName, true, customer);

                Console.WriteLine("Waiting for message...");
                Console.ReadKey();
            }
        }
    }

    private void CustomerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var encoded = Encoding.UTF8.GetString(e.Body.ToArray());

        Console.WriteLine(encoded);
    }
}