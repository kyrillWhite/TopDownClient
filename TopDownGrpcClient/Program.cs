using System;
using Grpc.Net.Client;
using Microsoft.VisualBasic;
using TopDownGrpcGameServer;

namespace TopDownGrpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            var begin = DateAndTime.Now;
            int count = 500;
            for (int i = 0; i < count; i++)
            {
                string query = "something";
                var q = new HelloRequest() { Name = query };
                var reply = client.SayHelloAsync(
                    new HelloRequest { Name = "GreeterClient" });
                var resp = reply.ResponseAsync;
                resp.Wait();
            }

            Console.WriteLine(((DateTime.Now - begin) / count).Milliseconds);
        }
    }
}
