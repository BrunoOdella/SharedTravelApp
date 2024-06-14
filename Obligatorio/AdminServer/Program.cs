using Grpc.Net.Client;
using GrpcServer;

namespace AdminServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Creando gRPC Client...");

            // Me conecto al servidor:
            using var channel = GrpcChannel.ForAddress("http://localhost:5035");
        }
    }
}
