// See https://aka.ms/new-console-template for more information

using RabbitMq.Rpc.Core;

using var client = new Client();
var result = client.Call("message 1");

Console.WriteLine(result);