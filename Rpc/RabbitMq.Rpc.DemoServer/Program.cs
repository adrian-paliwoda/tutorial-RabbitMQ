// See https://aka.ms/new-console-template for more information

using RabbitMq.Rpc.Core;

Console.WriteLine("Hello, World!");

using (var server = new RpcServer())
{
    server.Run();
    
}