using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.WorkQueues.Core;

public class Receiver
{
    private const string QueueName = "work_queues";

    public void Run()
    {
        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.QueueDeclare(QueueName, true, false, false, null);

                var customer = new EventingBasicConsumer(model);
                customer.Received += CustomerOnReceived;
                
                model.BasicConsume(QueueName, false, customer);

                Console.WriteLine("Waiting for message...");
                Console.ReadKey();
            }
        }
    }

    private void CustomerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var encoded = Encoding.UTF8.GetString(e.Body.ToArray());
        Thread.Sleep(TimeSpan.FromSeconds(5));
        if (sender is EventingBasicConsumer and IModel model)
        {
            model.BasicAck(e.DeliveryTag, false);    
        }
        Console.WriteLine(encoded);
    }
}