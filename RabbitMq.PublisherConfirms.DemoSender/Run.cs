using RabbitMq.PublisherConfirms.Core.BlockingForSingle;

namespace RabbitMq.PublisherConfirms.DemoSender;

public static class Run
{
    public static void Single()
    {
        var sender = new Sender();
        sender.Send("Wiadomosc");
    }

    public static void Batches()
    {
        var sender = new Core.BlockingForBatches.Sender();
        sender.Send();
    }

    public static void Async()
    {
        var sender = new Core.Asynchronous.Sender();
        sender.Send("wiadomosc");
    }
}