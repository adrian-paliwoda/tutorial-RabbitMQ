using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.PublisherConfirms.Core;

public class Receiver
{
    private static int _numberOfReceivers = 0;
    private static readonly object SyncObject = new object();

    private readonly int _id;
    
    private const string ExchangeName = "publisher_confirms";

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
                model.ConfirmSelect();
                
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
            model.BasicAck(e.DeliveryTag, false);
        }

        Console.WriteLine("[{0}|{1}] --- {2}", _id, e.DeliveryTag, encoded);
    }
}