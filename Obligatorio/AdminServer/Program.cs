using Grpc.Net.Client;

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
                Console.WriteLine("2. Modificar un viaje");
                Console.WriteLine("3. Eliminar un viaje");
                Console.WriteLine("0. Salir");

                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        CreateTrip(client);
                        break;
                    case "2":
                        UpdateTrip(client);
                        break;
                    case "3":
                        DeleteTrip(client);
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

            var response = client.CreateTrip(request);

            Console.WriteLine("Viaje creado con éxito.");
        }

        private static void UpdateTrip(AdminGrpc.AdminGrpcClient client)
        {
            List<TripElem> tripElems = client.GetAllTrips(new Empty()).Trips.ToList();

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

            var response = client.UpdateTrip(request);

            Console.WriteLine("Viaje modificado con éxito.");
        }

        private static void DeleteTrip(AdminGrpc.AdminGrpcClient client)
        {
            List<TripElem> tripElems = client.GetAllTrips(new Empty()).Trips.ToList();

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

            var response = client.DeleteTrip(request);

            Console.WriteLine("Viaje eliminado con éxito.");
        }

    }
}
