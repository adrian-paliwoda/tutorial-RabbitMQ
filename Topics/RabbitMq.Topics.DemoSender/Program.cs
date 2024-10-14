using RabbitMq.Topics.Core;

var tasks = new List<Task>();

var routingKeysForTasks = new string[][]
{
    new string[]{"log.*"},
    new string[]{"log.info"},
    new string[]{"log.warning"},
    new string[]{"log.debug"},

};

for (int i = 0; i < routingKeysForTasks.Length; i++)
{
    var id = i;
    tasks.Add(new Task(() => Action(id, routingKeysForTasks[id])));
}

for (int i = 0; i < tasks.Count; i++)
{
    tasks[i].Start();
}

void Action(int id, string[] routingKeys)
{
    var receiver = new Sender(routingKeys);
    receiver.Send($"[{id}] --- Wiadomosc");
}


Task.WaitAll(tasks.ToArray());