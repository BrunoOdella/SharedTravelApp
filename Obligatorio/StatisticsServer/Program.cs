using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
            builder.Services.AddSingleton<ITripReportRepository, TripReportRepository>();
            builder.Services.AddSingleton<ILoginEventRepository, LoginEventRepository>();

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
            var tripReportRepository = app.Services.GetRequiredService<ITripReportRepository>();
            var loginEventRepository = app.Services.GetRequiredService<ILoginEventRepository>();

            Task.Run(() => ConnectAndReceiveTrips(tripRepository, tripReportRepository));
            Task.Run(() => ConnectAndReceiveLogins(loginEventRepository));

            app.Run();
        }

        public static async Task ConnectAndReceiveTrips(ITripRepository tripRepository, ITripReportRepository tripReportRepository)
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

                var tripMessage = JsonSerializer.Deserialize<TripMessage>(message);

                var trip = tripMessage.Trip;
                trip.SetGuid(tripMessage.TripId);

                if (tripMessage != null)
                {
                    switch (tripMessage.Operation)
                    {
                        case "Create":
                            lock (tripRepository)
                            {
                                tripRepository.Add(trip);
                            }
                            break;
                        case "Update":
                            lock (tripRepository)
                            {
                                tripRepository.Update(trip);
                            }
                            break;
                        case "Delete":
                            lock (tripRepository)
                            {
                                tripRepository.Delete(tripMessage.TripId);
                            }
                            break;
                    }

                    List<TripReport> reportsToUpdate;
                    lock (tripReportRepository)
                    {
                        reportsToUpdate = tripReportRepository.GetAllReports().Where(r => !r.IsReady).ToList();
                    }

                    foreach (var report in reportsToUpdate)
                    {
                        if (report.Trips.Count < report.RequiredTrips)
                        {
                            report.Trips.Add(tripMessage.Trip);
                        }

                        if (report.Trips.Count >= report.RequiredTrips)
                        {
                            report.IsReady = true;
                            report.ReadyAt = DateTime.UtcNow;
                            lock (tripReportRepository)
                            {
                                tripReportRepository.UpdateReport(report);
                            }
                        }
                    }
                }
            };

            channel.BasicConsume(queue: "trips",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        public static async Task ConnectAndReceiveLogins(ILoginEventRepository loginEventRepository)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "logins",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var loginEvent = JsonSerializer.Deserialize<LoginEvent>(message);
                Console.WriteLine($"Received login event: User {loginEvent.UserId} at {loginEvent.Timestamp}");

                lock (loginEventRepository)
                {
                    loginEventRepository.Add(loginEvent);
                }
            };

            channel.BasicConsume(queue: "logins",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
