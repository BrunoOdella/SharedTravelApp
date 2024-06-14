using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Threading.Tasks;

namespace AdminServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5074");
            var client = new AdminGrpc.AdminGrpcClient(channel);

            while (true)
            {
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("1. Crear un nuevo viaje");
                // Puedes agregar más opciones aquí
                Console.WriteLine("0. Salir");

                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        CreateTrip(client);
                        break;
                    case "0":
                        Console.WriteLine("Saliendo...");
                        return;
                    default:
                        Console.WriteLine("Opción no válida. Intente de nuevo.");
                        break;
                }
            }
        }

        private static void CreateTrip(AdminGrpc.AdminGrpcClient client)
        {
            Console.WriteLine("Ingrese los detalles del nuevo viaje:");

            Console.Write("Owner ID: ");
            string ownerId = Console.ReadLine();

            Console.Write("Origen: ");
            string origin = Console.ReadLine();

            Console.Write("Destino: ");
            string destination = Console.ReadLine();

            Console.Write("Fecha de salida (yyyy-MM-dd HH:mm): ");
            string departure = Console.ReadLine();

            Console.Write("Número total de asientos: ");
            int totalSeats = int.Parse(Console.ReadLine());

            Console.Write("Precio por pasajero: ");
            float pricePerPassenger = float.Parse(Console.ReadLine());

            Console.Write("¿Se permiten mascotas? (true/false): ");
            bool petsAllowed = bool.Parse(Console.ReadLine());

            var request = new CreateTripRequest
            {
                OwnerId = ownerId,
                Origin = origin,
                Destination = destination,
                Departure = departure,
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats,
                PricePerPassenger = pricePerPassenger,
                PetsAllowed = petsAllowed
            };

            var response = client.CreateTrip(request);

            Console.WriteLine("Viaje creado con éxito.");
        }
    }
}
