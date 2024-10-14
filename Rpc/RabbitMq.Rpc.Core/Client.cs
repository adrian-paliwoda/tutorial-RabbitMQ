using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.Rpc.Core;

public class Client : IDisposable
{
    private const string DefaultServerName = "rpc_server";
    private readonly IConnection _connection;
    private readonly string _correlationId;
    private readonly IModel _model;
    private readonly string? _queueName;
    private readonly BlockingCollection<string> _results;

    private readonly string _serverQueueName;
    private bool _disposed;

    public Client(string? serverQueueName = null)
    {
        _serverQueueName = string.IsNullOrEmpty(serverQueueName) ? DefaultServerName : serverQueueName;
        _correlationId = Guid.NewGuid().ToString();
        _results = new BlockingCollection<string>();

        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        _connection = connectionFactory.CreateConnection();
        _model = _connection.CreateModel();
        _queueName = _model.QueueDeclare().QueueName;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    public string Call(string message)
    {
        var basicProperties = _model.CreateBasicProperties();
        basicProperties.ReplyTo = _queueName;
        basicProperties.CorrelationId = _correlationId;

        Send(_serverQueueName, message, basicProperties);
        return Receive();
    }

    private string Receive()
    {
        Console.WriteLine("Run receiver");

        var customer = new EventingBasicConsumer(_model);
        customer.Received += CustomerOnReceived;

        _model.BasicConsume(_queueName, false, customer);

        return _results.Take();
    }


    private void Send(string queueName, string message, IBasicProperties basicProperties)
    {
        if (!string.IsNullOrEmpty(queueName) && !string.IsNullOrEmpty(message))
        {
            var encoded = Encoding.UTF8.GetBytes(message);
            _model.BasicPublish("", queueName, basicProperties, new ReadOnlyMemory<byte>(encoded));
        }
    }

    private void CustomerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        if (e.BasicProperties.CorrelationId == _correlationId)
        {
            var decoded = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine(decoded);

            _model.BasicAck(e.DeliveryTag, false);

            Console.WriteLine("Work done.");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            _model?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}