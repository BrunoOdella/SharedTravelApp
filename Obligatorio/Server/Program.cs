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
        static readonly UserRepository userRepository = new UserRepository();
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
        }

        private static void load()
        {
            //load users
            UserContext context = UserContext.CreateInsance();
            UserContext.LoadUsersFromTxt();
            //
            loadTrips(context);
            loadCalification();
        }

        private static void loadCalification()
        {
            CalificationContext context = CalificationContext.CreateInsance();
            CalificationContext.LoadCalificationsFromTxt(); 
        }
        private static string ReceiveMessageFromClient(NetworkHelper networkHelper)
        {
            byte[] usernameInBytes = networkHelper.Receive(Protocol.DataLengthSize);
            int usernameLength = BitConverter.ToInt32(usernameInBytes);
            byte[] usernameBufferInBytes = networkHelper.Receive(usernameLength);
            return Encoding.UTF8.GetString(usernameBufferInBytes);
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
                    Console.WriteLine("Eligio la opcion 1");
                    break;
                case 2:
                    Console.WriteLine("Eligio la opcion 2");
                    break;
                case 3:
                    Console.WriteLine("Eligio la opcion 3");
                    ModifyTrip(networkHelper, socket, user);
                    break;
                case 4:
                    Console.WriteLine("Eligio la opcion 4");
                    break;
                case 5:
                    Console.WriteLine("Eligio la opcion 5");
                    break;
                case 6:
                    Console.WriteLine("Eligio la opcion 6");
                    break;
                case 7:
                    Console.WriteLine("Eligio la opcion 7");
                    break;
                case 8:
                    Console.WriteLine("Eligio la opcion 8");
                    break;
                case 9:
                    Console.WriteLine("Eligio la opcion 9");
                    break;
                default: break;
            }

        }

        private static void ModifyTrip(NetworkHelper networkHelper, Socket socket, User user)
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
                SendMessageToClient("No hay viajes publicados para fechas futuras.", networkHelper);
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
            var tripSelected = map[Int32.Parse(selected) - 1];
            SendMessageToClient($"Viaje seleccionado\n" +
                $"Origen: {{actualTrip.Origin}}, Destino: {{actualTrip.Destination}}" +
                $" y Fecha {{actualTrip.Departure.ToString()}}", networkHelper);


        }

        private static bool AuthenticateUser(string username, string password)
        {
            var allUsers = userRepository.GetAll();

            var authenticatedUser = allUsers.FirstOrDefault(u => u.Name == username && u._password == password);

            return authenticatedUser != null;
        }
    }
}
