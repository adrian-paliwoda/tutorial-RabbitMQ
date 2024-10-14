using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.Routing.Core;

public class Receiver
{
    private const string ExchangeName = "routing";
    private static int _numberOfReceivers;
    private static readonly object SyncObject = new();

    private readonly int _id;
    private readonly string[] _routingKeys;

    public Receiver(string[] routingKeys)
    {
        lock (SyncObject)
        {
            _id = _numberOfReceivers;
            _numberOfReceivers++;
        }

        _routingKeys = routingKeys.Length > 0 ? routingKeys : new[] {""};
    }

    public void Run()
    {
        Console.WriteLine("Run receiver [{0}]", _id);

        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);

                var queueName = model.QueueDeclare().QueueName;
                Bind(model, queueName);

                var customer = new EventingBasicConsumer(model);
                customer.Received += CustomerOnReceived;

                model.BasicConsume(queueName, false, customer);

                Console.WriteLine("[{0}] --- Waiting for message...", _id);
                Console.ReadKey();
            }
        }
    }

    private void Bind(IModel model, string queueName)
    {
        for (var i = 0; i < _routingKeys.Length; i++)
        {
            model.QueueBind(queueName, ExchangeName, _routingKeys[i]);
        }
    }

    private void CustomerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var encoded = Encoding.UTF8.GetString(e.Body.ToArray());
        if (sender is EventingBasicConsumer and IModel model)
        {
            model.BasicAck(e.DeliveryTag, false);
        }

        Console.WriteLine("[{0}] --- {1}", _id, encoded);
    }
}