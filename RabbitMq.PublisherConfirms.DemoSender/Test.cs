using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMq.PublisherConfirms.DemoSender;

// From RabbitMQ tutorial
// Code from repository
public class RmqTest
{
    private const int MessageCount = 50_000;

    public void Run()
    {
        // PublishMessagesIndividually();
        // PublishMessagesInBatch();
        HandlePublishConfirmsAsynchronously();
    }

    private static IConnection CreateConnection()
    {
        var factory = new ConnectionFactory {HostName = "localhost"};
        return factory.CreateConnection();
    }

    private static void PublishMessagesIndividually()
    {
        using var connection = CreateConnection();
        using var channel = connection.CreateModel();

        // declare a server-named queue
        var queueName = channel.QueueDeclare().QueueName;
        channel.ConfirmSelect();

        var timer = new Stopwatch();
        timer.Start();
        for (var i = 0; i < MessageCount; i++)
        {
            var body = Encoding.UTF8.GetBytes(i.ToString());
            channel.BasicPublish("", queueName, null, body);
            channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
        }

        timer.Stop();
        Console.WriteLine($"Published {MessageCount:N0} messages individually in {timer.ElapsedMilliseconds:N0} ms");
    }

    private static void PublishMessagesInBatch()
    {
        using var connection = CreateConnection();
        using var channel = connection.CreateModel();

        // declare a server-named queue
        var queueName = channel.QueueDeclare().QueueName;
        channel.ConfirmSelect();

        var batchSize = 100;
        var outstandingMessageCount = 0;
        var timer = new Stopwatch();
        timer.Start();
        for (var i = 0; i < MessageCount; i++)
        {
            var body = Encoding.UTF8.GetBytes(i.ToString());
            channel.BasicPublish("", queueName, null, body);
            outstandingMessageCount++;

            if (outstandingMessageCount == batchSize)
            {
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                outstandingMessageCount = 0;
            }
        }

        if (outstandingMessageCount > 0)
        {
            channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
        }

        timer.Stop();
        Console.WriteLine($"Published {MessageCount:N0} messages in batch in {timer.ElapsedMilliseconds:N0} ms");
    }

    private static void HandlePublishConfirmsAsynchronously()
    {
        using var connection = CreateConnection();
        using var channel = connection.CreateModel();

        // declare a server-named queue
        var queueName = channel.QueueDeclare().QueueName;
        channel.ConfirmSelect();

        var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

        void cleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
        {
            if (multiple)
            {
                var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                foreach (var entry in confirmed)
                {
                    outstandingConfirms.TryRemove(entry.Key, out _);
                }
            }
            else
            {
                outstandingConfirms.TryRemove(sequenceNumber, out _);
            }
        }

        channel.BasicAcks += (sender, ea) =>
        {
            Console.WriteLine(ea.DeliveryTag);
            cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
        };
        channel.BasicNacks += (sender, ea) =>
        {
            outstandingConfirms.TryGetValue(ea.DeliveryTag, out var body);
            Console.WriteLine(
                $"Message with body {body} has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
            cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
        };

        var timer = new Stopwatch();
        timer.Start();
        for (var i = 0; i < MessageCount; i++)
        {
            var body = i.ToString();
            outstandingConfirms.TryAdd(channel.NextPublishSeqNo, i.ToString());
            channel.BasicPublish("", queueName, null, Encoding.UTF8.GetBytes(body));
        }

        if (!WaitUntil(60, () => outstandingConfirms.IsEmpty))
        {
            throw new Exception("All messages could not be confirmed in 60 seconds");
        }

        timer.Stop();
        Console.WriteLine(
            $"Published {MessageCount:N0} messages and handled confirm asynchronously {timer.ElapsedMilliseconds:N0} ms");
    }

    private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
    {
        var waited = 0;
        while (!condition() && waited < numberOfSeconds * 1000)
        {
            Thread.Sleep(100);
            waited += 100;
        }

        return condition();
    }
}