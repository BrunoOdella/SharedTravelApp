﻿using Grpc.Core;
using Grpc.Net.Client;

namespace AdminServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5074");
            var client = new AdminGrpc.AdminGrpcClient(channel);

            while (true)
            {
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("1. Crear un nuevo viaje");
                Console.WriteLine("2. Modificar un viaje");
                Console.WriteLine("3. Eliminar un viaje");
                Console.WriteLine("4. Obtener los siguientes N viajes creados");
                Console.WriteLine("0. Salir");

                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await CreateTripAsync(client);
                        break;
                    case "2":
                        await UpdateTripAsync(client);
                        break;
                    case "3":
                        await DeleteTripAsync(client);
                        break;
                    case "4":
                        GetNextNTripsAsync(client);
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

        private static async Task GetNextNTripsAsync(AdminGrpc.AdminGrpcClient client)
        {
            Console.WriteLine("Ingrese la cantidad de viajes que quiere resivir.");

            string cantidad;

            do             {
                cantidad = Console.ReadLine();
            } while (!int.TryParse(cantidad, out _) && int.Parse(cantidad) > 0);

            var request = new NextTripsRequest
            {
                Quantity = int.Parse(cantidad)
            };

            using var response = client.GetNextTrips(request);

            await foreach (var trip in response.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine();
                Console.WriteLine("Viaje creado");
                Console.WriteLine($"N°: {trip.Index}");
                Console.WriteLine($"Origen: {trip.Origin}");
                Console.WriteLine($"Destino: {trip.Destination}");
                Console.WriteLine($"Fecha de salida: {trip.Departure}");
                Console.WriteLine($"Precio por pasajero: {trip.PricePerPassenger}");
                Console.WriteLine($"¿Se permiten mascotas? {trip.PetsAllowed}");
                Console.WriteLine();
            }
        }

        private static async Task CreateTripAsync(AdminGrpc.AdminGrpcClient client)
        {
            Console.WriteLine("Ingrese los detalles del nuevo viaje:");

            Console.Write("Owner Username: ");
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

            var response = await client.CreateTripAsync(request);

            Console.WriteLine("Viaje creado con éxito.");
        }

        private static async Task UpdateTripAsync(AdminGrpc.AdminGrpcClient client)
        {
            var tripResponse = await client.GetAllTripsAsync(new Empty());
            List<TripElem> tripElems = tripResponse.Trips.ToList();

            foreach (var tripElem in tripElems)
            {
                Console.WriteLine($"ID: {tripElem.Index}");
                Console.WriteLine($"Origen: {tripElem.Origin}");
                Console.WriteLine($"Destino: {tripElem.Destination}");
                Console.WriteLine($"Fecha de salida: {tripElem.Departure}");
                Console.WriteLine($"Precio por pasajero: {tripElem.PricePerPassenger}");
                Console.WriteLine($"¿Se permiten mascotas? {tripElem.PetsAllowed}");
                Console.WriteLine();
            }

            Console.WriteLine("Ingrese el numero del viaje a modificar:");
            int tripId = int.Parse(Console.ReadLine());

            Console.Write("Origen: ");
            string origin = Console.ReadLine();

            Console.Write("Destino: ");
            string destination = Console.ReadLine();

            Console.Write("Fecha de salida (yyyy-MM-dd HH:mm): ");
            string departure = Console.ReadLine();

            Console.Write("Precio por pasajero: ");
            float pricePerPassenger = float.Parse(Console.ReadLine());

            Console.Write("¿Se permiten mascotas? (true/false): ");
            bool petsAllowed = bool.Parse(Console.ReadLine());

            var request = new UpdateTripRequest
            {
                Index = tripId,
                Origin = origin,
                Destination = destination,
                Departure = departure,
                PetsAllowed = petsAllowed,
                PricePerPassenger = pricePerPassenger,
            };

            var response = await client.UpdateTripAsync(request);

            Console.WriteLine("Viaje modificado con éxito.");
        }

        private static async Task DeleteTripAsync(AdminGrpc.AdminGrpcClient client)
        {
            var tripResponse = await client.GetAllTripsAsync(new Empty());
            List<TripElem> tripElems = tripResponse.Trips.ToList();

            foreach (var tripElem in tripElems)
            {
                Console.WriteLine($"ID: {tripElem.Index + 1}");
                Console.WriteLine($"Origen: {tripElem.Origin}");
                Console.WriteLine($"Destino: {tripElem.Destination}");
                Console.WriteLine($"Fecha de salida: {tripElem.Departure}");
                Console.WriteLine($"Precio por pasajero: {tripElem.PricePerPassenger}");
                Console.WriteLine($"¿Se permiten mascotas? {tripElem.PetsAllowed}");
                Console.WriteLine();
            }

            Console.WriteLine("Ingrese el numero del viaje a eliminar:");
            int tripId = int.Parse(Console.ReadLine());

            var request = new TripIndex
            {
                Index = tripId-1
            };

            var response = await client.DeleteTripAsync(request);

            Console.WriteLine("Viaje eliminado con éxito.");
        }

    }
}
