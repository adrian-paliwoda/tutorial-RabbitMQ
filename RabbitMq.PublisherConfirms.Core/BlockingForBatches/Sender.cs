using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMq.PublisherConfirms.Core.BlockingForBatches;

public class Sender
{
    private const string ExchangeName = "publisher_confirms";
    private const int BatchSize = 50;
    private const int DefaultNumberOfMessages = 100;

    public void Send(string[]? messages = null)
    {
        ConcurrentDictionary<ulong, byte[]> sentMessages = new();
        messages = FillMessagesIfEmpty(messages);

        var connectionFactory = new ConnectionFactory {HostName = "localhost"};
        using (var connection = connectionFactory.CreateConnection())
        {
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
                model.ConfirmSelect();
                
                for (var i = 0; i < messages.Length; i++)
                {
                    var encoded = Encoding.UTF8.GetBytes(messages[i]);
                
                    if (sentMessages.TryAdd(model.NextPublishSeqNo, encoded))
                    {
                        model.BasicPublish(ExchangeName, "", null, new ReadOnlyMemory<byte>(encoded));
                    }
                
                    lock (sentMessages)
                    {
                        if (sentMessages.Count >= BatchSize)
                        {
                            model.WaitForConfirmsOrDie(new TimeSpan(0, 0, 15));
                            sentMessages.Clear();
                        }
                    }
                }

                lock (sentMessages)
                {
                    if (sentMessages.Count > 0)
                    {
                        model.WaitForConfirmsOrDie(new TimeSpan(0, 0, 15));
                        sentMessages.Clear();
                    }
                }
            }
        }
    }

    private string[] FillMessagesIfEmpty(string[]? messages)
    {
        var properMessages = messages ?? Array.Empty<string>();
        
        if (messages == null || messages.Length < DefaultNumberOfMessages)
        {
            properMessages = new string[DefaultNumberOfMessages];
            int indexStart = 0;
            if (messages != null)
            {
                indexStart = messages.Length;
                Array.Copy(messages,properMessages, indexStart);
            }

            for (int i = indexStart; i < properMessages.Length; i++)
            {
                properMessages[i] = "Example of messages in queue....";
            }
        }

        return properMessages;
    }
}