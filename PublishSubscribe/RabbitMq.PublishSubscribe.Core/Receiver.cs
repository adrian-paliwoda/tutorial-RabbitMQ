using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.PublishSubscribe.Core;

public class Receiver
{
    private static int _numberOfReceivers = 0;
    private static readonly object SyncObject = new object();

    private readonly int _id;
    
    private const string ExchangeName = "publish_subscribe";

    public Receiver()
    {
        lock (SyncObject)
        {
            _id = _numberOfReceivers;
            _numberOfReceivers++;
        }
    }

    public void Run()
    {
        Console.WriteLine("Run receiver [{0}]", _id);
        
        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                var queueName = model.QueueDeclare().QueueName;
                
                model.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
                model.QueueBind(queueName, ExchangeName, "");

                var customer = new EventingBasicConsumer(model);
                customer.Received += CustomerOnReceived;

                model.BasicConsume(queueName, false, customer);

                Console.WriteLine("[{0}] --- Waiting for message...", _id);
                Console.ReadKey();
            }
        }
    }

    private void CustomerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var encoded = Encoding.UTF8.GetString(e.Body.ToArray());
        if (sender is EventingBasicConsumer and IModel model)
        {
            Thread.Sleep(new TimeSpan(0, 0, 10));
            model.BasicAck(e.DeliveryTag, false);
        }

        Console.WriteLine("[{0}] --- {1}", _id, encoded);
    }
}