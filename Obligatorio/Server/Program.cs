using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common;
using Common.Interfaces;
using System.Threading;
using DataAcces;
using Server.BL;
using Microsoft.VisualBasic.FileIO;
using Server.DataAcces.Repositories;
using Server.BL.Repositories;
using System.Security;
using System.Collections.Generic;

namespace Server
{
    internal class Program
    {
        static readonly ISettingsManager SettingsMgr = new SettingsManager();
        static readonly IUserRepository userRepository = new UserRepository();
        static readonly ITripRepository ITripRepo = new TripRepository();
        static readonly ICalificationRepository calificationRepository = new CalificationRepository();
        static readonly IUserRepository IUserRepo = new UserRepository();
        static readonly ICalificationRepository ICalificationRepo = new CalificationRepository();

        static bool acceptClients = true;
        static List<Task> clientTasks = new List<Task>();


        static async Task Main(string[] args)
        {
            load();

            try
            {
                var localEndPoint = new IPEndPoint(
                    IPAddress.Parse(SettingsMgr.ReadSetting(ServerConfig.ServerIpConfigKey)),
                    int.Parse(SettingsMgr.ReadSetting(ServerConfig.SeverPortConfigKey))
                );

                TcpListener listener = new TcpListener(localEndPoint);
                listener.Start();
                Console.WriteLine("Esperando clientes ...");

                var consoleTask = Task.Run(() => HandleConsoleInput());

                int clients = 1;

                while (acceptClients || clientTasks.Count > 0)
                {
                    if (acceptClients && listener.Pending())
                    {
                        // Aceptar el cliente y enviar un mensaje de bienvenida
                        TcpClient client = await listener.AcceptTcpClientAsync();
                        var stream = client.GetStream();
                        var writer = new StreamWriter(stream);
                        writer.WriteLine("Bienvenido al servidor");
                        writer.Flush();

                        // Crea una nueva tarea para manejar al cliente y añádela a la lista
                        var tcs = new TaskCompletionSource<bool>();
                        var clientTask = Task.Run(() => HandleClientAsync(client, clientTasks.Count, tcs));
                        clientTasks.Add(clientTask);

                        // Cuando la tarea se complete, la removemos de la lista
                        clientTask.ContinueWith(t =>
                        {
                            clientTasks.Remove(clientTask);
                            Console.WriteLine("Clientes conectados: " + clientTasks.Count);
                        });
                    }
                    else if (!acceptClients && listener.Pending())
                    {
                        // Aceptar el cliente y enviar un mensaje de rechazo
                        TcpClient client = listener.AcceptTcpClient();
                        var stream = client.GetStream();
                        var writer = new StreamWriter(stream);
                        writer.WriteLine("El servidor no está aceptando nuevos clientes");
                        writer.Flush();
                        client.Close();
                    }
                }
                
                Console.WriteLine("Servidor apagado.....");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }


        private static void loadTrips(UserContext userContenxt)
        {
            TripContext context = TripContext.CreateInsance();
            context.LoadTripsFromTxt(userContenxt);
            loadCalification(userContenxt, context);
        }

        private static void load()
        {
            //load users
            UserContext context = UserContext.CreateInsance();
            UserContext.LoadUsersFromTxt();
            //
            loadTrips(context);
        }

        private static void loadCalification(UserContext userContenxt, TripContext tripContext)
        {
            CalificationContext context = CalificationContext.CreateInsance();
            CalificationContext.LoadCalificationsFromTxt(userContenxt, tripContext); 
        }
        private static async Task<string> ReceiveMessageFromClientAsync(NetworkHelper networkHelper)
        {
            byte[] messageInBytes = await networkHelper.ReceiveAsync(Protocol.DataLengthSize);
            int messageLength = BitConverter.ToInt32(messageInBytes);
            byte[] messageBufferInBytes = await networkHelper.ReceiveAsync(messageLength);
            return Encoding.UTF8.GetString(messageBufferInBytes);
        }

        private static async Task SendMessageToClientAsync(string message, NetworkHelper networkHelper)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes(message);
            int responseLength = responseBuffer.Length;
            byte[] responseLengthInBytes = BitConverter.GetBytes(responseLength);
            await networkHelper.SendAsync(responseLengthInBytes);
            await networkHelper.SendAsync(responseBuffer);
        }

        private static async Task HandleClientAsync(TcpClient client, int clientNumber, TaskCompletionSource<bool> tcs)
        {
            Console.WriteLine($"El cliente {clientNumber} se conectó");
            NetworkHelper networkHelper = new NetworkHelper(client);

            try
            {
                User user = new User();
                bool validUser = false;

                while (!validUser)
                {
                    string username = await ReceiveMessageFromClientAsync(networkHelper);
                    string password = await ReceiveMessageFromClientAsync(networkHelper);

                    validUser = AuthenticateUser(username, password);

                    string response = "OK";
                    if (!validUser)
                    {
                        response = "ERROR";
                        await SendMessageToClientAsync(response, networkHelper);
                    }
                    else
                    {
                        Console.WriteLine($"Logueado el usuario {clientNumber} con éxito");

                        var allUsers = userRepository.GetAll();
                        var authenticatedUser = allUsers.FirstOrDefault(u => u.Name == username && u._password == password);
                        Guid userId = authenticatedUser.GetGuid();

                        user = userRepository.Get(userId);
                        await SendMessageToClientAsync(response, networkHelper);

                        await GoToOptionAsync(networkHelper, client, user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el cliente {clientNumber}: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
            tcs.SetResult(true);
        }

        private static async Task GoToOptionAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            bool salir = false;
            while (!salir)
            {
                string option = await ReceiveMessageFromClientAsync(networkHelper);
                int opt = Int32.Parse(option);
                switch (opt)
                {
                    case 1:
                        await PublishTripAsyncAsync(networkHelper, client, user);
                        break;
                    case 2:
                        await JoinTripAsync(networkHelper, client, user);
                        break;
                    case 3:
                        await ModifyTripAsync(networkHelper, client, user);
                        break;
                    case 4:
                        await WithdrawFromTripAsync(networkHelper, client, user);
                        break;
                    case 5:
                        await TripSearchAsyncAsync(networkHelper, client, user);
                        break;
                    case 6:
                        await ViewTripInfoAsyncAsync(networkHelper, client, user);
                        break;
                    case 7:
                        await RateDriverAsyncAsync(networkHelper, client, user);
                        break;
                    case 8:
                        await ViewDriverRatingsAsyncAsync(networkHelper, client, user);
                        break;
                    case 9:
                        await DeleteTripAsyncAsync(networkHelper, client, user);
                        break;
                    case 10:
                        salir = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private static async Task DeleteTripAsyncAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            try
            {
                List<Trip> trips = ITripRepo.GetTripsByOwner(user.GetGuid());
                List<Trip> ActualsTrips = new List<Trip>();

                foreach (Trip trip in trips)
                {
                    if (trip.Departure > DateTime.Now)
                        ActualsTrips.Add(trip);
                }

                if (ActualsTrips.Count == 0)
                {
                    await SendMessageToClientAsync("0", networkHelper);
                    return;
                }

                await SendMessageToClientAsync($"{ActualsTrips.Count}", networkHelper);


                int count = 1;
                foreach (var trip in ActualsTrips)
                {
                    await SendMessageToClientAsync($"{count} | Origen: {trip.Origin}, Destino: {trip.Destination}, Fecha de salida: {trip.Departure}", networkHelper);
                    count++;
                }

                string selected = await ReceiveMessageFromClientAsync(networkHelper);
                if (selected == "salir")
                    return;

                int pos = int.Parse(selected) - 1;

                ITripRepo.Remove(ActualsTrips[pos]);

                await SendMessageToClientAsync("Se elimino el viaje", networkHelper);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al eliminar un viaje: " + e.Message);
                await SendMessageToClientAsync("Error al eliminar un viaje.", networkHelper);
            }
            
        }


        public static async Task ViewDriverRatingsAsyncAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            await SendUsernamesToClient(networkHelper);
            string username = await ReceiveMessageFromClientAsync(networkHelper);
            User foundUser = userRepository.GetUserByUsername(username);

            if (foundUser == null)
            {
                await SendMessageToClientAsync("Error: Usuario no encontrado.", networkHelper);
                return;
            }

            List<Calification> califications = GetDriverCalifications(foundUser.GetGuid());
            string response = califications.Count > 0 ? FormatCalifications(califications)
                : "Este conductor no tiene calificaciones disponibles.";
            await SendMessageToClientAsync(response, networkHelper);
        }

        private static async Task RateDriverAsyncAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            try
            {
                List<Trip> trips = ITripRepo.GetAll(user._id);
                List<User> users = new List<User>();
                foreach (Trip trip in trips)
                {
                    var actual = IUserRepo.Get(trip._owner);
                    users.Add(actual);
                }

                if (users.Count == 0)
                {
                    await SendMessageToClientAsync("EMPTY", networkHelper);
                    return;
                }
                await SendMessageToClientAsync($"{users.Count}", networkHelper);
                int count = 1;
                for (int i = 0; i < trips.Count && i < users.Count; i++)
                {
                    var actualUser = users[i];
                    var actualTrip = trips[i];
                    await SendMessageToClientAsync($"{count} | {actualUser.Name}, Origen del viaje: {actualTrip.Origin}" +
                                        $", Destino del viaje {actualTrip.Destination}, Fecha del viaje " +
                                        $"{actualTrip.Departure}", networkHelper);
                    count++;
                }

                string response = await ReceiveMessageFromClientAsync(networkHelper);
                if(response == "salir")
                    return;

                int selected = int.Parse(response);

                float score = float.Parse(await ReceiveMessageFromClientAsync(networkHelper));

                string comment = await ReceiveMessageFromClientAsync(networkHelper);

                ICalificationRepo.Add(new Calification(users[selected - 1].GetGuid(), trips[selected - 1].GetGuid(), score, comment));

                var ActualOwner= users[selected - 1];
                ActualOwner.AddScore(score);
                IUserRepo.Update(ActualOwner);
                await SendMessageToClientAsync($"Calificacion cargada", networkHelper);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al calificar un conductor: " + e.Message);
                await SendMessageToClientAsync("Error al calificar un conductor.", networkHelper);

            }
        }

        private static async Task ViewTripInfoAsyncAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {

            List<Trip> trips;
            try
            {
                trips = await TripSearchAsyncAsync(networkHelper, client, user);
                int amountOfTrips = trips.Count;
                await SendMessageToClientAsync(amountOfTrips.ToString(), networkHelper);

                if (amountOfTrips > 0)
                {
                    string selectedTripIndexStr = await ReceiveMessageFromClientAsync(networkHelper);
                    int selectedTripIndex = int.Parse(selectedTripIndexStr) - 1;

                    if (selectedTripIndex >= 0 && selectedTripIndex < trips.Count)
                    {
                        Trip selectedTrip = trips[selectedTripIndex];
                        await SendAllTripInfo(networkHelper, selectedTrip);

                        string download = await ReceiveMessageFromClientAsync(networkHelper);
                        if (download == "si")
                        {
                            await SendStreamToClient(networkHelper, selectedTrip.Photo);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Selección de viaje inválida.");
                    }
                }

                
            }
            catch (Exception ex)
            {
                // Enviar mensaje al cliente sobre la falta de viajes disponibles
                await SendMessageToClientAsync("ERROR" + ex.Message, networkHelper);
            }
        }

        private static async Task JoinTripAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            
            string response = "";

            List<Trip> trips= await ViewAllFutureTripsAsync(networkHelper, client, user);
            //hacer una funcion que filtre
            trips = ITripRepo.FilterByDeparture(trips);

            await SendMessageToClientAsync(trips.Count.ToString(), networkHelper);
            //le mando la cant dfe trips al cliente asi sabe si decirle al user que elija uno
            if (trips.Count > 0)
            {
                try
                {

                    string selectedTripIndexStr = await ReceiveMessageFromClientAsync(networkHelper);
                    if (selectedTripIndexStr == "exit")
                    {
                        return;
                    }
                    int selectedTripIndex = int.Parse(selectedTripIndexStr) - 1;

                    if (selectedTripIndex >= 0 && selectedTripIndex < trips.Count)
                    {
                        Trip selectedTrip = trips[selectedTripIndex];
                        Console.WriteLine("El viaje seleccionado es: " + selectedTrip);

                        try
                        {
                            Trip tripToJoin = ITripRepo.Get(selectedTrip._id);

                            if (ITripRepo.isJoined(tripToJoin._id, user._id))
                            {
                                response = "Usted ya forma parte de este viaje, no es posible unirlo";
                            }

                            if (ITripRepo.isOwner(tripToJoin._id, user._id))
                            {
                                response = "Usted es el dueño de este viaje, no es posible unirlo";
                            }



                            if (!ITripRepo.isOwner(tripToJoin._id, user._id) && !ITripRepo.isJoined(tripToJoin._id, user._id))
                            {
                                tripToJoin.AvailableSeats--;

                                tripToJoin._passengers.Add(user._id);

                                ITripRepo.Update(tripToJoin);

                                response = "Se ha unido correctamente al viaje.";


                            }
                        }
                        catch (Exception ex)
                        {
                            response = "Error al unirse al viaje: " + ex.Message;
                            await SendMessageToClientAsync(response, networkHelper);
                        }
                    }
                    else
                    {
                        response = "Seleccion de viaje invalida";
                    }
                    await SendMessageToClientAsync(response, networkHelper);


                }
                catch (Exception ex)
                {
                    await SendMessageToClientAsync("ERROR" + ex.Message, networkHelper);
                }
            }
            
            
        }

        private static async Task WithdrawFromTripAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            try
            {
                List<Trip> trips = ITripRepo.GetAll();
                List<Trip> UserInTrips = new List<Trip>();
                foreach (Trip trip in trips)
                {
                    if (trip.PassangerInTrip(user.GetGuid()) && trip.Departure > DateTime.Now)
                        UserInTrips.Add(trip);
                }

                if (!UserInTrips.Any())
                {
                    await SendMessageToClientAsync("EMPTY", networkHelper);
                    return;
                }

                await SendMessageToClientAsync($"{UserInTrips.Count}", networkHelper);
                int count = 1;
                foreach (Trip trip in UserInTrips)
                {
                    await SendMessageToClientAsync($"{count} | Origen: {trip.Destination}, Destino: {trip.Destination}, " +
                                        $"Fecha de salida: {trip.Departure}", networkHelper);
                    count++;
                }

                string selected = await ReceiveMessageFromClientAsync(networkHelper);
                if(selected == "salir")
                    return;

                int pos = int.Parse(selected) - 1;

                Trip TripSelected = UserInTrips[pos];
                TripSelected.Withdraw(user.GetGuid());

                await SendMessageToClientAsync("OK", networkHelper);
            }
            catch (Exception ex)
            {
                await SendMessageToClientAsync("Error al eliminar el pasajero del viaje.", networkHelper);
            }
            
        }

        private static async Task ModifyTripAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            try
            {
                var trips = ITripRepo.GetAll();
                var map = new Dictionary<int, Guid>();
                int count = 1;
                foreach(var trip in trips)
                {
                    if (trip.GetOwner() == user.GetGuid() && trip.Departure > DateTime.Now)
                    {
                        map.Add(count++, trip.GetGuid());
                    }
                }
                if(map.Count == 0) 
                {
                    await SendMessageToClientAsync("EMPTY", networkHelper);
                    return;
                }
                else
                {
                    await SendMessageToClientAsync($"{map.Count}", networkHelper);
                    foreach (var trip in map)
                    {
                        var actualTrip = ITripRepo.Get(trip.Value);
                        await SendMessageToClientAsync($"Viaje {trip.Key} | Origen: {actualTrip.Origin}, Destino: {actualTrip.Destination}" +
                            $" y Fecha {actualTrip.Departure.ToString()}", networkHelper);
                    }
                }
                string selected = await ReceiveMessageFromClientAsync(networkHelper);
                if (selected != "null")
                {
                    var tripSelected = map[Int32.Parse(selected)];
                    var selectedTrip = ITripRepo.Get(tripSelected);
                    await SendMessageToClientAsync($"Viaje seleccionado\n" +
                        $"Origen: {selectedTrip.Origin}, Destino: {selectedTrip.Destination}" +
                        $" y Fecha {selectedTrip.Departure.ToString()}", networkHelper);

                    //recibir cada elemento del trip, si es EMPTY mantener el actual, sino cambiarlo por el recibido
                    //y previo hacer las comprobaciones adecuadas


                    string newOrigin = await ReceiveMessageFromClientAsync(networkHelper);
                    if (newOrigin != "EMPTY")
                    {
                        selectedTrip.Origin = newOrigin;
                    }

                    string newDestination = await ReceiveMessageFromClientAsync(networkHelper);
                    if (newDestination != "EMPTY")
                    {
                        selectedTrip.Destination = newDestination;
                    }

                    DateTime newDepartureTime;
                    string aux = await ReceiveMessageFromClientAsync(networkHelper);
                    if (aux != "EMPTY")
                    {
                        selectedTrip.Departure = DateTime.Parse(aux);
                    }

                    string newPet = await ReceiveMessageFromClientAsync(networkHelper);
                    if (newPet != "EMPTY")
                    {
                        selectedTrip.Pet = bool.Parse(newPet);
                    }

                    string newPricePerSeat = await ReceiveMessageFromClientAsync(networkHelper);
                    if (newPricePerSeat != "EMPTY")
                    {
                        selectedTrip.PricePerPassanger = int.Parse(newPricePerSeat);
                    }

                    string newPhoto;
                    if (bool.Parse(await ReceiveMessageFromClientAsync(networkHelper)))
                    {
                        newPhoto = await ReceiveStreamFromClientAsync(networkHelper);
                    }

                    await SendMessageToClientAsync($"Viaje actualizado", networkHelper);

                    ITripRepo.Update(selectedTrip);
                }
                
            }
            catch (Exception ex)
            {
                await SendMessageToClientAsync("Error al modificar el viaje.", networkHelper);
            }
        }

        private static bool AuthenticateUser(string username, string password)
        {
            var allUsers = userRepository.GetAll();

            var authenticatedUser = allUsers.FirstOrDefault(u => u.Name == username && u._password == password);

            return authenticatedUser != null;
        }

        

        
        private static async Task PublishTripAsyncAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            try
            {
                string origin = await ReceiveMessageFromClientAsync(networkHelper);
                string destination = await ReceiveMessageFromClientAsync(networkHelper);
                DateTime departure = DateTime.Parse(await ReceiveMessageFromClientAsync(networkHelper));
                int totalSeats = int.Parse(await ReceiveMessageFromClientAsync(networkHelper));
                float pricePerPassanger = float.Parse(await ReceiveMessageFromClientAsync(networkHelper));
                bool pet = bool.Parse(await ReceiveMessageFromClientAsync(networkHelper));
                string photo = await ReceiveStreamFromClientAsync(networkHelper);

                Trip newTrip = new Trip()
                {
                    Origin = origin,
                    Destination = destination,
                    Departure = departure,
                    TotalSeats = totalSeats,
                    AvailableSeats = totalSeats,
                    PricePerPassanger = pricePerPassanger,
                    Pet = pet,
                    Photo = photo,
                };

                newTrip.SetOwner(user.GetGuid());
                ITripRepo.Add(newTrip);

                await SendMessageToClientAsync("Viaje publicado con éxito.", networkHelper);
            }
            catch (Exception ex)
            {
                await SendMessageToClientAsync("Error al publicar el viaje.", networkHelper);
            }
        }

        private static async Task<List<Trip>> TripSearchAsyncAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            string option = await ReceiveMessageFromClientAsync(networkHelper);
            int opt = Int32.Parse(option);
            switch (opt)
            {
                case 1:
                    return await ViewAllTripsAsync(networkHelper, client, user);
                case 2:
                    return await ViewTripsFilteredByOriginAndDestinationAsync(networkHelper, client, user);
                case 3:
                    return await ViewAllTripsFilteredPetFriendlyAsync(networkHelper, client, user);
                case 4:
                    return await ViewAllFutureTripsAsync(networkHelper, client, user);
                default:
                    return new List<Trip>();
            }
        }



        private static async Task<List<Trip>> ViewAllTripsAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            List<Trip> allTrips = ITripRepo.GetAll();
            string tripCount = allTrips.Count.ToString();
            await SendMessageToClientAsync(tripCount, networkHelper);

            for (int i = 0; i < allTrips.Count; i++)
            {
                Trip trip = allTrips[i];
                string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                await SendMessageToClientAsync(tripString, networkHelper);
            }
            return allTrips;
        }


        private static async Task<List<Trip>> ViewTripsFilteredByOriginAndDestinationAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            string origin = await ReceiveMessageFromClientAsync(networkHelper);
            string destination = await ReceiveMessageFromClientAsync(networkHelper);

            List<Trip> tripsToOriginAndDestination;
            try
            {
                tripsToOriginAndDestination = ITripRepo.GetAllTripsToOriginAndDestination(origin, destination);

                string tripCount = tripsToOriginAndDestination.Count.ToString();
                await SendMessageToClientAsync(tripCount, networkHelper);

                for (int i = 0; i < tripsToOriginAndDestination.Count; i++)
                {
                    Trip trip = tripsToOriginAndDestination[i];
                    string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                    await SendMessageToClientAsync(tripString, networkHelper);
                }
                return tripsToOriginAndDestination;
            }
            catch (Exception ex)
            {
                await SendMessageToClientAsync("ERROR" + ex.Message, networkHelper);
                return new List<Trip> { };
                
            }
        }

        private static async Task<List<Trip>> ViewAllTripsFilteredPetFriendlyAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            string option = await ReceiveMessageFromClientAsync(networkHelper);
            bool petFriendly = false;
            if(option == "SI")  
            {
                petFriendly = true;
            }

            List<Trip> trips;
            try
            {
                trips = ITripRepo.GetTripsFilteredByPetFriendly(petFriendly);

                string tripCount = trips.Count.ToString();
                await SendMessageToClientAsync(tripCount, networkHelper);

                for (int i = 0; i < trips.Count; i++)
                {
                    Trip trip = trips[i];
                    string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                    await SendMessageToClientAsync(tripString, networkHelper);
                }
                return trips;
            }
            catch (Exception ex)
            {
                await SendMessageToClientAsync("ERROR" + ex.Message, networkHelper);
                return new List<Trip> { };
            }
        }
        private static async Task<List<Trip>> ViewAllFutureTripsAsync(NetworkHelper networkHelper, TcpClient client, User user)
        {
            List<Trip> allTrips = ITripRepo.GetAll();
            List<Trip> trips = ITripRepo.FilterByDeparture(allTrips);

            string tripCount = trips.Count.ToString();
            await SendMessageToClientAsync(tripCount, networkHelper);

            for (int i = 0; i < trips.Count; i++)
            {
                Trip trip = trips[i];
                string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                await SendMessageToClientAsync(tripString, networkHelper);
            }

            return trips;
        }


        private static string SerializeTrip(Trip trip)
        {
            return $"Origen:{trip.Origin} -> Destino: {trip.Destination}, Fecha y hora de salida:{trip.Departure}";
        }


        private static async Task<string> ReceiveStreamFromClientAsync(NetworkHelper networkHelper)
        {
            byte[] fileNameLengthInBytes = await networkHelper.ReceiveAsync(Protocol.fileNameLengthSize);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthInBytes);

            byte[] fileNameInBytes = await networkHelper.ReceiveAsync(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameInBytes);

            byte[] fileLengthInBytes = await networkHelper.ReceiveAsync(Protocol.fileSizeLength);
            long fileLength = BitConverter.ToInt64(fileLengthInBytes);

            long numberOfParts = Protocol.numberOfParts(fileLength);

            int currentPart = 1;
            int offset = 0;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = "ReceivedFiles";
            string saveDirectory = Path.Combine(basePath, relativePath);

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            string savePath = Path.Combine(saveDirectory, fileName);

            FileStreamHelper fs = new FileStreamHelper();
            while (offset < fileLength)
            {
                bool isLastPart = (currentPart == numberOfParts);
                int numberOfBytesToReceive = isLastPart ? (int)(fileLength - offset) : Protocol.MaxPartSize;

                byte[] buffer = await networkHelper.ReceiveAsync(numberOfBytesToReceive);

                await fs.WriteAsync(savePath, buffer);

                currentPart++;
                offset += numberOfBytesToReceive;
            }

            return savePath;
        }






        private static async Task SendStreamToClient(NetworkHelper networkHelper, string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileName = fileInfo.Name;
            byte[] fileNameInBytes = Encoding.UTF8.GetBytes(fileName);
            int fileNameLength = fileNameInBytes.Length;
            byte[] fileNameLengthInBytes = BitConverter.GetBytes(fileNameLength);
            await networkHelper.SendAsync(fileNameLengthInBytes);

            await networkHelper.SendAsync(fileNameInBytes);

            long fileLength = fileInfo.Length;
            byte[] fileLengthInBytes = BitConverter.GetBytes(fileLength);
            await networkHelper.SendAsync(fileLengthInBytes);


            long numberOfParts = Protocol.numberOfParts(fileLength);

            int currentPart = 1;
            int offset = 0;

            FileStreamHelper fs = new FileStreamHelper();
            while (offset < fileLength)
            {
                bool isLastPart = (currentPart == numberOfParts);

                int numberOfBytesToSend;
                if (isLastPart)
                {
                    numberOfBytesToSend = (int)(fileLength - offset);
                }
                else
                {
                    numberOfBytesToSend = Protocol.MaxPartSize;
                }

                byte[] bytesReadFromDisk = await fs.ReadAsync(filePath, offset, numberOfBytesToSend);

                await networkHelper.SendAsync(bytesReadFromDisk);
                currentPart++;
                offset += numberOfBytesToSend;
            }
            Console.WriteLine($"Termine de enviar archivo {filePath}, de tamaño {fileLength} bytes");

        }
        public static async Task SendUsernamesToClient(NetworkHelper networkHelper)
        {
            List<User> users = userRepository.GetAll();
            StringBuilder usernames = new StringBuilder();
            foreach (var user in users)
            {
                usernames.AppendLine(user.Name);
            }
            await SendMessageToClientAsync(usernames.ToString(), networkHelper);
        }

        private static async Task SendAllTripInfo(NetworkHelper networkHelper, Trip trip)
        {
            await SendMessageToClientAsync(trip.Origin, networkHelper);
            await SendMessageToClientAsync(trip.Destination, networkHelper);
            await SendMessageToClientAsync(trip.Departure.ToString(), networkHelper);
            await SendMessageToClientAsync(trip.AvailableSeats.ToString(), networkHelper);
            await SendMessageToClientAsync(trip.PricePerPassanger.ToString(), networkHelper);
            await SendMessageToClientAsync(trip.Pet.ToString(), networkHelper);
        }



        public static async Task ProcessUserSelection(NetworkHelper networkHelper)
        {
            string selectedUsername = await ReceiveMessageFromClientAsync(networkHelper);
            User user = userRepository.GetUserByUsername(selectedUsername);

            if (user == null)
            {
                await SendMessageToClientAsync("Error: El usuario no existe.", networkHelper);
                return;
            }

            List<Calification> califications = GetDriverCalifications(user._id);
            if (califications.Count == 0)
            {
                await SendMessageToClientAsync("El usuario no tiene calificaciones disponibles.", networkHelper);
            }
            else
            {
                string response = FormatCalifications(califications);
                await SendMessageToClientAsync(response, networkHelper);
            }
        }

        private static List<Calification> GetDriverCalifications(Guid driverId)
        {
            List<Trip> driverTrips = ITripRepo.GetTripsByOwner(driverId);
            List<Calification> driverCalifications = new List<Calification>();

            foreach (var trip in driverTrips)
            {
                driverCalifications.AddRange(calificationRepository.GetCalificationsByTripId(trip._id));
            }

            return driverCalifications;
        }




        public static string FormatCalifications(List<Calification> califications)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var calification in califications)
            {
                var trip = ITripRepo.Get(calification.GetTrip());
                sb.AppendLine($"Origen del viaje: {trip.Origin}, Destino del viaje: {trip.Destination}, Score: {calification.Score}, Comment: {calification.Comment}");
            }
            return sb.ToString();
        }

        static void HandleConsoleInput()
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "salir")
                {
                    Console.WriteLine("Dejando de aceptar nuevos clientes......");
                    Console.WriteLine("Clientes conectados: " + clientTasks.Count);
                    acceptClients = false;
                }
            }
        }


    }
}
