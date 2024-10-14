using System.Security.Cryptography.X509Certificates;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq.Rpc.Core;

public class RpcServer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _model;
    private readonly string queueName = "rpc_server";
    private bool _disposed;


    public RpcServer()
    {
        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        _connection = connectionFactory.CreateConnection();
        _model = _connection.CreateModel();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Run()
    {
        Receive(queueName);
    }

    private void Receive(string queueName)
    {
        if (string.IsNullOrEmpty(queueName))
        {
            return;
        }

        Console.WriteLine("Run receiver");

        _model.QueueDeclare(queueName, false, false, false, null);
        _model.BasicQos(0, 1, false);

        var customer = new EventingBasicConsumer(_model);
        customer.Received += CustomerOnReceived;

        _model.BasicConsume(queueName, false, customer);

        Console.WriteLine("Waiting for message...");
        Console.ReadKey();
    }

    private void Send(string queueName, string message, IBasicProperties? basicProperties = null)
    {
        if (!string.IsNullOrEmpty(queueName) && !string.IsNullOrEmpty(message))
        {
            var encoded = Encoding.UTF8.GetBytes(message);
            _model.BasicPublish("", queueName, basicProperties, new ReadOnlyMemory<byte>(encoded));
        }
    }

    private void CustomerOnReceived(object? sender, BasicDeliverEventArgs e)
    {
        var decoded = Encoding.UTF8.GetString(e.Body.ToArray());
        
        var basicProperties = _model.CreateBasicProperties();
        basicProperties.CorrelationId = e.BasicProperties.CorrelationId;

        string result = String.Empty;

        try
        {
            result = GetResult();
        }
        catch (Exception)
        {
            result = "Error";
        }
        finally
        {
            if (e.BasicProperties != null)
            {
                Send(e.BasicProperties.ReplyTo, result, basicProperties);
                _model.BasicAck(e.DeliveryTag, false);
            }
        }
        
        Console.WriteLine(decoded);
    }

    private string GetResult()
    {
        return "Success. Work done";
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