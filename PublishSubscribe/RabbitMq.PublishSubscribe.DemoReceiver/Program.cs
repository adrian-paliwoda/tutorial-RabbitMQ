using RabbitMq.PublishSubscribe.Core;

const int numberOfReceivers = 5;
var tasks = new Task[numberOfReceivers];

for (int i = 0; i < numberOfReceivers; i++)
{
    tasks[i] = new Task(Action);
}

for (int i = 0; i < numberOfReceivers; i++)
{
    tasks[i].Start();
}

void Action()
{
    var receiver = new Receiver();
    receiver.Run();
}



Task.WaitAll(tasks);