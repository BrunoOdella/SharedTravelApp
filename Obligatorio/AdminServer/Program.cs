using Grpc.Core;
using Grpc.Net.Client;
using System.Globalization;

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
                Console.WriteLine("4. Ver las calificaciones de un viaje");
                Console.WriteLine("5. Obtener los siguientes N viajes creados");
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
                        await GetTripCalificationsAsync(client);
                        break;
                    case "5":
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

            do
            {
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

            Console.WriteLine("Usuarios disponibles en el sistema:");
            var usersResponse = await client.GetAllUsersAsync(new Empty());
            List<UserElem> userElems = usersResponse.Users.ToList();

            for (int i = 0; i < userElems.Count; i++)
            {
                Console.WriteLine($"{userElems[i].Username}");
            }

            string ownerId = await PromptForUsername("Nombre de usuario del propietario", client);

            string origin = PromptForNonEmptyString("Origen: ");

            string destination = PromptForNonEmptyString("Destino: ");

            DateTime departure = PromptForFutureDateTime("Fecha de salida (yyyy-MM-dd HH)");
            string departureString = departure.ToString("yyyy-MM-dd HH:mm");

            int totalSeats = PromptForInt("Número total de asientos: ");

            float pricePerPassenger = PromptForFloat("Precio por pasajero: ");

            bool petsAllowed = PromptForBoolean("¿Se permiten mascotas? (si/no)");

            var request = new CreateTripRequest
            {
                OwnerId = ownerId,
                Origin = origin,
                Destination = destination,
                Departure = departureString,
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

            int tripId = PromptForValidId("ID del viaje: ", tripElems);

            bool modificar;
            string origin = tripElems[tripId].Origin;
            string destination = tripElems[tripId].Destination;
            DateTime departure= DateTime.Parse(tripElems[tripId].Departure);
            float pricePerSeat= tripElems[tripId].PricePerPassenger;
            bool petsAllowed = tripElems[tripId].PetsAllowed;

            modificar = PromptForBoolean("¿Modificar Origen? (SI/NO)");
            if (modificar)
            {
                origin = PromptForNonEmptyString("Nuevo Origen:");
            }
            modificar = PromptForBoolean("¿Modificar Destino? (SI/NO)");
            if (modificar)
            {
                destination = PromptForNonEmptyString("Nuevo Destino:");
            }
            modificar = PromptForBoolean("¿Modificar Fecha de Salida? (SI/NO)");
            if (modificar)
            {
                departure = PromptForFutureDateTime("Nueva Fecha de Salida (yyyy-MM-dd HH)");
            }
            modificar = PromptForBoolean("¿Modificar Precio por Pasajero? (SI/NO)");
            if (modificar)
            {
                pricePerSeat = PromptForFloat("Nuevo Precio por Pasajero:");
            }
            modificar = PromptForBoolean("¿Modificar ¿Se permiten mascotas? (SI/NO)");
            if (modificar)
            {
                petsAllowed = PromptForBoolean("¿Se permiten mascotas? (si/no)");
            }
            string departureString = departure.ToString("yyyy-MM-dd HH:mm");

            var request = new UpdateTripRequest
            {
                Index = tripId,
                Origin = origin,
                Destination = destination,
                Departure = departureString,
                PetsAllowed = petsAllowed,
                PricePerPassenger = pricePerSeat,
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
                Console.WriteLine($"ID: {tripElem.Index}");
                Console.WriteLine($"Origen: {tripElem.Origin}");
                Console.WriteLine($"Destino: {tripElem.Destination}");
                Console.WriteLine($"Fecha de salida: {tripElem.Departure}");
                Console.WriteLine($"Precio por pasajero: {tripElem.PricePerPassenger}");
                Console.WriteLine($"¿Se permiten mascotas? {tripElem.PetsAllowed}");
                Console.WriteLine();
            }

            int tripId = PromptForValidId("ID del viaje a eliminar: ", tripElems);

            var request = new TripIndex
            {
                Index = tripId
            };

            var response = await client.DeleteTripAsync(request);

            Console.WriteLine("Viaje eliminado con éxito.");
        }

        private static async Task GetTripCalificationsAsync(AdminGrpc.AdminGrpcClient client)
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

            int tripId = PromptForValidId("ID del viaje: ", tripElems);
            Console.WriteLine();
            Console.WriteLine("Lista de calificaciones:");
            Console.WriteLine();
            var request = new TripIndex
            {
                Index = tripId
            };

            var response = await client.GetTripCalificationsAsync(request);

            foreach (var calification in response.Ratings)
            {
                Console.WriteLine($"Usuario: {calification.Username}");
                Console.WriteLine($"Calificación: {calification.Score}");
                Console.WriteLine($"Comentario: {calification.Comment}");
                Console.WriteLine();
            }
        }


        private static string PromptForNonEmptyString(string prompt)
        {
            string input;
            do
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine().Trim();
            } while (string.IsNullOrEmpty(input));
            return input;
        }

        private static DateTime PromptForFutureDateTime(string prompt)
        {
            DateTime inputDate;
            while (true)
            {
                Console.WriteLine(prompt);
                string input = Console.ReadLine().Trim();
                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out inputDate))
                {
                    inputDate = new DateTime(inputDate.Year, inputDate.Month, inputDate.Day, inputDate.Hour, 0, 0);
                    if (inputDate > DateTime.Now)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Por favor, introduzca una fecha y hora futura.");
                        Console.WriteLine("");
                    }
                }
                else
                {
                    Console.WriteLine("Formato de fecha y hora inválido. Use el formato 'yyyy-MM-dd HH'.");
                    Console.WriteLine("");
                }
            }
            return inputDate;
        }

        private static int PromptForInt(string prompt)
        {
            int inputValue;
            while (true)
            {
                Console.WriteLine(prompt);
                if (int.TryParse(Console.ReadLine().Trim(), out inputValue))
                {
                    return inputValue;
                }
                Console.WriteLine("Número inválido, por favor reintente.");
                Console.WriteLine("");
            }
        }

        private static bool PromptForBoolean(string prompt)
        {
            string input;
            do
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine().Trim().ToLower();
            } while (input != "si" && input != "no");
            return input == "si";
        }

        private static float PromptForFloat(string prompt)
        {
            float inputValue;
            while (true)
            {
                Console.WriteLine(prompt);
                if (float.TryParse(Console.ReadLine().Trim(), out inputValue))
                {
                    return inputValue;
                }
                Console.WriteLine("Precio inválido, por favor reintente.");
                Console.WriteLine("");
            }
        }

        private static async Task<string> PromptForUsername(string prompt, AdminGrpc.AdminGrpcClient client)
        {
            string input;
            var usersResponse = await client.GetAllUsersAsync(new Empty());
            List<UserElem> userElems = usersResponse.Users.ToList();
            while (true)
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine().Trim();
                if (userElems.Any(u => u.Username == input))
                {
                    return input;
                }
                Console.WriteLine("Usuario no encontrado, por favor reintente.");
                Console.WriteLine("");
            }
        }

        private static int PromptForValidId(string prompt, List<TripElem> tripElems)
        {
            int inputValue;
            while (true)
            {
                Console.WriteLine(prompt);
                if (int.TryParse(Console.ReadLine().Trim(), out inputValue) && inputValue > 0 && inputValue <= tripElems.Count-1)
                {
                    return inputValue;
                }
                Console.WriteLine("ID inválido, por favor reintente.");
                Console.WriteLine("");
            }
        }
    }
}
