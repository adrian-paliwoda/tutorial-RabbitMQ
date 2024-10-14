﻿using RabbitMq.Routing.Core;

var tasks = new List<Task>();

var routingKeysForTasks = new string[][]
{
    new string[]{"info","warning"},
    new string[]{"info"},
    new string[]{"warning"},
    new string[]{"debug"},
};


for (int i = 0; i < routingKeysForTasks.Length; i++)
{
    var i1 = i;
    tasks.Add(new Task(() => Action(routingKeysForTasks[i1])));
}


for (int i = 0; i < tasks.Count; i++)
{
    tasks[i].Start();
}

void Action(string[] routingKeys)
{
        var receiver = new Receiver(routingKeys);
        receiver.Run();
}

Task.WaitAll(tasks.ToArray());