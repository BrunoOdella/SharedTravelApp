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

namespace Server
{
    internal class Program
    {
        static readonly ISettingsManager SettingsMgr = new SettingsManager();
        static readonly IUserRepository userRepository = new UserRepository();
        static readonly ITripRepository ITripRepo = new TripRepository();


        static void Main(string[] args)
        {
            
            load();

            try
            {
                var localEndPoint = new IPEndPoint(
                                         IPAddress.Parse(SettingsMgr.ReadSetting(ServerConfig.ServerIpConfigKey)),
                                         int.Parse(SettingsMgr.ReadSetting(ServerConfig.SeverPortConfigKey))
                                    );

                Socket listenerSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(10);
                Console.WriteLine("Esperando clientes ...");

                int clients = 1;

                while (true)
                {
                    Socket clientSocket = listenerSocket.Accept(); // Bloqueante
                    int clientNumber = clients;
                    Thread clientThread = new Thread(() => HandleClient(clientSocket, clientNumber));
                    clientThread.Name = "Cliente #" + clientNumber;
                    clientThread.Start();
                    clients++;
                }
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
        private static string ReceiveMessageFromClient(NetworkHelper networkHelper)
        {
            byte[] messageInBytes = networkHelper.Receive(Protocol.DataLengthSize);
            int messageLength = BitConverter.ToInt32(messageInBytes);
            byte[] messageBufferInBytes = networkHelper.Receive(messageLength);
            return Encoding.UTF8.GetString(messageBufferInBytes);
        }

        private static void SendMessageToClient(string message, NetworkHelper networkHelper)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes(message);
            int responseLength = responseBuffer.Length;
            byte[] responseLengthInBytes = BitConverter.GetBytes(responseLength);
            networkHelper.Send(responseLengthInBytes);
            networkHelper.Send(responseBuffer);
        }

        public static void HandleClient(Socket clientSocket, int clientNumber)
        {
            Console.WriteLine($"El cliente {clientNumber} se conectó");
            NetworkHelper networkHelper = new NetworkHelper(clientSocket);

            try
            {
                User user = new User();
                bool validUser = false;

                while (!validUser)
                {
                    string username = ReceiveMessageFromClient(networkHelper);
                    /*
                    if (username == "EXIT")
                    {
                        Console.WriteLine(ReceiveMessageFromClient(networkHelper));
                        allSockets.Remove(socketClient);
                        break;
                    }
                    */
                    string password = ReceiveMessageFromClient(networkHelper);

                    validUser = AuthenticateUser(username, password);

                    string response = "OK";
                    if (!validUser)
                    {
                        response = "ERROR";
                        SendMessageToClient(response, networkHelper);
                    }
                    else
                    {
                        Console.WriteLine($"Logueado el usuario {clientNumber} con exito");

                        //ver de arreglar esto (sacarlo para afuera en una funcion o algo)
                        var allUsers = userRepository.GetAll();

                        var authenticatedUser = allUsers.FirstOrDefault(u => u.Name == username && u._password == password);
                        Guid userId = authenticatedUser.GetGuid();

                        user = userRepository.Get(userId);
                        SendMessageToClient(response, networkHelper);
                        string option = ReceiveMessageFromClient(networkHelper);

                        GoToOption(option,networkHelper, clientSocket, user);
                        
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el cliente {clientNumber}: " + ex.Message);
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }

        private static void GoToOption(string option,NetworkHelper networkHelper,Socket socket, User user)
        {
            int opt = Int32.Parse(option);
            switch (opt)
            {
                case 1:
                    PublishTrip(networkHelper, socket, user);
                    break;
                case 2:
                    JoinTrip(networkHelper, socket, user);
                    break;

                case 3:
                    ModifyTrip(networkHelper, socket, user);
                    break;
                case 4:
                    break;
                case 5:
                    TripSearch(networkHelper, socket, user);
                    break;
                case 6:
                    ViewTripInfo(networkHelper, socket, user);
                    break;
                case 7:
                    
                    break;
                case 8:
                    break;
                case 9:
                    break;
                default: break;
            }

        }

        private static void ViewTripInfo(NetworkHelper networkHelper, Socket socket, User user)
        {

            List<Trip> trips;
            try
            {
                trips = ITripRepo.GetAll();

                string tripCount = trips.Count.ToString();
                SendMessageToClient(tripCount, networkHelper);

                for (int i = 0; i < trips.Count; i++)
                {
                    Trip trip = trips[i];
                    string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                    SendMessageToClient(tripString, networkHelper);
                }

                string selectedTripIndexStr = ReceiveMessageFromClient(networkHelper);
                int selectedTripIndex = int.Parse(selectedTripIndexStr) - 1;

                if (selectedTripIndex >= 0 && selectedTripIndex < trips.Count)
                {
                    Trip selectedTrip = trips[selectedTripIndex];
                    Console.WriteLine("El viaje seleccionado es: " + selectedTrip);
                    SendMessageToClient(AllTripInfo(selectedTrip), networkHelper);

                    string download = ReceiveMessageFromClient(networkHelper);
                    if (download == "si") 
                    {
                        SendStreamToClient(networkHelper, selectedTrip.Photo);
                    }
                }
                else
                {
                    Console.WriteLine("Selección de viaje inválida.");
                }
            }
            catch (Exception ex)
            {
                // Enviar mensaje al cliente sobre la falta de viajes disponibles
                SendMessageToClient("ERROR" + ex.Message, networkHelper);
                string nextOption = ReceiveMessageFromClient(networkHelper);
                GoToOption(nextOption, networkHelper, socket, user);
            }
        }

        private static void JoinTrip(NetworkHelper networkHelper, Socket socket, User user)
        {
            string origin = ReceiveMessageFromClient(networkHelper);
            string destination = ReceiveMessageFromClient(networkHelper);

            List<Trip> tripsToOriginAndDestination;
            try
            {
                tripsToOriginAndDestination = ITripRepo.GetAllTripsToOriginAndDestination(origin, destination);

                string tripCount = tripsToOriginAndDestination.Count.ToString();
                SendMessageToClient(tripCount, networkHelper);

                for (int i = 0; i < tripsToOriginAndDestination.Count; i++)
                {
                    Trip trip = tripsToOriginAndDestination[i];
                    string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                    SendMessageToClient(tripString, networkHelper);
                }

                string selectedTripIndexStr = ReceiveMessageFromClient(networkHelper);
                int selectedTripIndex = int.Parse(selectedTripIndexStr) - 1;

                if (selectedTripIndex >= 0 && selectedTripIndex < tripsToOriginAndDestination.Count)
                {
                    Trip selectedTrip = tripsToOriginAndDestination[selectedTripIndex];
                    Console.WriteLine("El viaje seleccionado es: " + selectedTrip);

                    try
                    {
                        Trip tripToJoin = ITripRepo.Get(selectedTrip._id);
                        //MANEJAR LOS CASOS DE QUE:
                        //YA ESTA UNIDO A ESE TRIP
                        //ES EL OWNER DE ESE TRIP

                        //este if  (que checkea lo de available seats) lo podria sacar porque ya lo chequeo en el respositorio
                        if (tripToJoin.AvailableSeats > 0)
                        {
                            tripToJoin.AvailableSeats--;

                            tripToJoin._passengers.Add(user._id);

                            ITripRepo.Update(tripToJoin);

                            Console.WriteLine("Se ha unido correctamente al viaje.");

                            string nextOption = ReceiveMessageFromClient(networkHelper);
                            GoToOption(nextOption, networkHelper, socket, user);
                        }
                        else
                        {
                            Console.WriteLine("No hay asientos disponibles en este viaje.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al unirse al viaje: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Selección de viaje inválida.");
                }
            }
            catch (Exception ex)
            {
                // Enviar mensaje al cliente sobre la falta de viajes disponibles
                SendMessageToClient("ERROR" + ex.Message, networkHelper);
                string nextOption = ReceiveMessageFromClient(networkHelper);
                GoToOption(nextOption, networkHelper, socket, user);
            }
        }

        private static void ModifyTrip(NetworkHelper networkHelper, Socket socket, User user)
        {
            try
            {
                var trips = ITripRepo.GetAll();
                var map = new Dictionary<int, Guid>();
                int count = 1;
                foreach(var trip in trips)
                {
                    if(trip.GetOwner() == user.GetGuid() && trip.Departure > DateTime.Now)
                    {
                        map.Add(count++, trip.GetGuid());
                    }
                }
                if(map.Count == 0) 
                {
                    SendMessageToClient("EMPTY", networkHelper);
                }
                else
                {
                    SendMessageToClient($"{map.Count}", networkHelper);
                    foreach (var trip in map)
                    {
                        var actualTrip = ITripRepo.Get(trip.Value);
                        SendMessageToClient($"Viaje {trip.Key} | Origen: {actualTrip.Origin}, Destino: {actualTrip.Destination}" +
                            $" y Fecha {actualTrip.Departure.ToString()}", networkHelper);
                    }
                }
                string selected = ReceiveMessageFromClient(networkHelper);
                var tripSelected = map[Int32.Parse(selected)];
                var selectedTrip = ITripRepo.Get(tripSelected);
                SendMessageToClient($"Viaje seleccionado\n" +
                    $"Origen: {selectedTrip.Origin}, Destino: {selectedTrip.Destination}" +
                    $" y Fecha {selectedTrip.Departure.ToString()}", networkHelper);

                //recibir cada elemento del trip, si es EMPTY mantener el actual, sino cambiarlo por el recibido
                //y previo hacer las comprobaciones adecuadas


                string newOrigin = ReceiveMessageFromClient(networkHelper);
                if (newOrigin != "EMPTY")
                {
                    selectedTrip.Origin = newOrigin;
                }

                string newDestination = ReceiveMessageFromClient(networkHelper);
                if (newDestination != "EMPTY")
                {
                    selectedTrip.Destination = newDestination;
                }

                string newPricePerSeat = ReceiveMessageFromClient(networkHelper);
                if (newPricePerSeat != "EMPTY")
                {
                    selectedTrip.PricePerPassanger = int.Parse(newPricePerSeat);
                }

                string newPet = ReceiveMessageFromClient(networkHelper);
                if (newPet != "EMPTY")
                {
                    selectedTrip.Pet = bool.Parse(newPet);
                }

                DateTime newDepartureTime;
                if (DateTime.TryParse(ReceiveMessageFromClient(networkHelper), out newDepartureTime))
                {
                    selectedTrip.Departure = newDepartureTime;
                }

                string newPhoto;
                if (bool.Parse(ReceiveMessageFromClient(networkHelper)))
                {
                    newPhoto = ReceiveStreamFromClient(networkHelper);
                }

                SendMessageToClient($"Viaje actualizado", networkHelper);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al modificar el viaje: " + ex.Message);
                SendMessageToClient("Error al modificar el viaje.", networkHelper);
            }
        }

        private static bool AuthenticateUser(string username, string password)
        {
            var allUsers = userRepository.GetAll();

            var authenticatedUser = allUsers.FirstOrDefault(u => u.Name == username && u._password == password);

            return authenticatedUser != null;
        }

        

        
        private static void PublishTrip(NetworkHelper networkHelper, Socket socket, User user)
        {
            try
            {
                string origin = ReceiveMessageFromClient(networkHelper);
                string destination = ReceiveMessageFromClient(networkHelper);
                DateTime departure = DateTime.Parse(ReceiveMessageFromClient(networkHelper));
                int totalSeats = int.Parse(ReceiveMessageFromClient(networkHelper));
                float pricePerPassanger = float.Parse(ReceiveMessageFromClient(networkHelper));
                bool pet = bool.Parse(ReceiveMessageFromClient(networkHelper));
                string photo = ReceiveStreamFromClient(networkHelper);

                Trip newTrip = new Trip()
                {
                    Origin = origin,
                    Destination = destination,
                    Departure = departure,
                    TotalSeats = totalSeats,
                    PricePerPassanger = pricePerPassanger,
                    Pet = pet,
                    Photo = photo,
                };

                TripRepository tripRepository = new TripRepository();
                tripRepository.Add(newTrip);

                SendMessageToClient("Viaje publicado con éxito.", networkHelper);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al publicar el viaje: " + ex.Message);
                SendMessageToClient("Error al publicar el viaje.", networkHelper);
            }
        }

        private static void TripSearch(NetworkHelper networkHelper, Socket socket, User user)
        {
            string option = ReceiveMessageFromClient(networkHelper);
            int opt = Int32.Parse(option);
            switch (opt)
            {
                case 1:
                    ViewAllTrips(networkHelper, socket, user);
                    break;
                case 2:
                    ViewTripsFilteredByOriginAndDestination(networkHelper, socket, user);
                    break;
                case 3:
                    ViewAllTripsFilteredPetFriendly(networkHelper, socket, user);
                    break;
                default: break;
            }
        }

        private static void ViewAllTrips(NetworkHelper networkHelper, Socket socket, User user)
        {
            List<Trip> allTrips;
            allTrips = ITripRepo.GetAll();

            string tripCount = allTrips.Count.ToString();
            SendMessageToClient(tripCount, networkHelper);

            for (int i = 0; i < allTrips.Count; i++)
            {
                Trip trip = allTrips[i];
                string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                SendMessageToClient(tripString, networkHelper);
            }
        }

        private static void ViewTripsFilteredByOriginAndDestination(NetworkHelper networkHelper, Socket socket, User user)
        {
            string origin = ReceiveMessageFromClient(networkHelper);
            string destination = ReceiveMessageFromClient(networkHelper);

            List<Trip> tripsToOriginAndDestination;
            try
            {
                tripsToOriginAndDestination = ITripRepo.GetAllTripsToOriginAndDestination(origin, destination);

                string tripCount = tripsToOriginAndDestination.Count.ToString();
                SendMessageToClient(tripCount, networkHelper);

                for (int i = 0; i < tripsToOriginAndDestination.Count; i++)
                {
                    Trip trip = tripsToOriginAndDestination[i];
                    string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                    SendMessageToClient(tripString, networkHelper);
                }
            }
            catch (Exception ex)
            {
                SendMessageToClient("ERROR" + ex.Message, networkHelper);
                string nextOption = ReceiveMessageFromClient(networkHelper);
                GoToOption(nextOption, networkHelper, socket, user);
            }
        }

        private static void ViewAllTripsFilteredPetFriendly(NetworkHelper networkHelper, Socket socket, User user)
        {
            string option = ReceiveMessageFromClient(networkHelper);
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
                SendMessageToClient(tripCount, networkHelper);

                for (int i = 0; i < trips.Count; i++)
                {
                    Trip trip = trips[i];
                    string tripString = $"{i + 1}: {SerializeTrip(trip)}";
                    SendMessageToClient(tripString, networkHelper);
                }
            }
            catch (Exception ex)
            {
                SendMessageToClient("ERROR" + ex.Message, networkHelper);
                string nextOption = ReceiveMessageFromClient(networkHelper);
                GoToOption(nextOption, networkHelper, socket, user);
            }
        }

        private static string SerializeTrip(Trip trip)
        {
            return $"Origen:{trip.Origin} -> Destino: {trip.Destination}, Fecha y hora de salida:{trip.Departure}";
        }


        private static string ReceiveStreamFromClient(NetworkHelper networkHelper)
        {
            byte[] fileNameLengthInBytes = networkHelper.Receive(Protocol.fileNameLengthSize);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthInBytes);

            byte[] fileNameInBytes = networkHelper.Receive(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameInBytes);

            byte[] fileLengthInBytes = networkHelper.Receive(Protocol.fileSizeLength);
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
                Console.WriteLine($"Recibiendo parte #{currentPart}, de {numberOfBytesToReceive} bytes");

                byte[] buffer = networkHelper.Receive(numberOfBytesToReceive);

                fs.Write(savePath, buffer);

                currentPart++;
                offset += numberOfBytesToReceive;
            }

            Console.WriteLine($"Archivo recibido completamente y guardado en {savePath}, tamaño total {fileLength} bytes");

            return savePath;
        }





        private static void SendStreamToClient(NetworkHelper networkHelper,string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileName = fileInfo.Name;
            byte[] fileNameInBytes = Encoding.UTF8.GetBytes(fileName);
            int fileNameLength = fileNameInBytes.Length;
            byte[] fileNameLengthInBytes = BitConverter.GetBytes(fileNameLength);
            networkHelper.Send(fileNameLengthInBytes);

            networkHelper.Send(fileNameInBytes);

            long fileLength = fileInfo.Length;
            byte[] fileLengthInBytes = BitConverter.GetBytes(fileLength);
            networkHelper.Send(fileLengthInBytes);


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
                Console.WriteLine($"Enviando parte #{currentPart}, de {numberOfBytesToSend} bytes");

                byte[] bytesReadFromDisk = fs.Read(filePath, offset, numberOfBytesToSend);

                networkHelper.Send(bytesReadFromDisk);
                currentPart++;
                offset += numberOfBytesToSend;
            }
            Console.WriteLine($"Termine de enviar archivo {filePath}, de tamaño {fileLength} bytes");

        }
        private static string AllTripInfo(Trip trip)
        {
            return $"Origen:{trip.Origin} -> Destino:{trip.Destination}" +
                $",Asientos Disponibles:{trip.AvailableSeats}" +
                $", Fecha y hora de salida:{trip.Departure}" +
                $", Cantidad de ascientos disponibles {trip.AvailableSeats}" +
                $", Precio {trip.PricePerPassanger} " +
                $", Se permiten mascotas: {trip.Pet} ";
        }
    }
}
