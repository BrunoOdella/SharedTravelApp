using GrpcServer.Server;
using GrpcServer.Services;

namespace GrpcServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //    app.Run();
            var oldServerTask = Task.Run(() => LaunchServer.Launch());

            // Start the gRPC server
            await StartGrpc(args);
        }

        public static async Task StartGrpc(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<AdminGrpcService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            await app.RunAsync();
        }

    }
}