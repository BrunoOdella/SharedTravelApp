
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using StatisticsServer.Repositories;
using System.Text;
using System.Text.Json;
using StatisticsServer.DTO;

namespace StatisticsServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddSingleton<ITripRepository, TripRepository>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            var tripRepository = app.Services.GetRequiredService<ITripRepository>();
            Task rabbit = Task.Run(() => ConnectAndReceive(tripRepository));

            app.Run();
        }

        public static async Task ConnectAndReceive(ITripRepository tripRepository)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "trips",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var trip = JsonSerializer.Deserialize<Trip>(message);
                Console.WriteLine($"Received trip: {trip.Origin} to {trip.Destination}");

                lock (tripRepository)
                {
                    tripRepository.Add(trip);
                }
            };

            channel.BasicConsume(queue: "trips",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
