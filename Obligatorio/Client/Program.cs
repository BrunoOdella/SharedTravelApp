using System.ComponentModel.Design;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common.Interfaces;
using Common;
using System.Diagnostics.Eventing.Reader;

namespace Client
{
    internal class Program
    {
        static readonly ISettingsManager SettingsMgr = new SettingsManager();
        static void Main(string[] args)
        {
            IPEndPoint local = new IPEndPoint(
                                        IPAddress.Parse(SettingsMgr.ReadSetting(ClientConfig.ClientIpConfigKey)),
                                        int.Parse(SettingsMgr.ReadSetting(ClientConfig.ClientPortConfigKey))
                                );
            IPEndPoint server = new IPEndPoint(
                                        IPAddress.Parse(SettingsMgr.ReadSetting(ClientConfig.ServerIpConfigKey)),
                                        int.Parse(SettingsMgr.ReadSetting(ClientConfig.SeverPortConfigKey))
                                );

            Socket soc = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            soc.Bind(local);
            try
            {
                
                soc.Connect(server);
                Console.WriteLine("Cliente conectado con el servidor");


                LogIn(soc);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                soc.Shutdown(SocketShutdown.Both);
                soc.Close();
            }
        }

        private static void LogIn(Socket socketClient)
        {
            NetworkHelper networkHelper = new NetworkHelper(socketClient);
            bool loggedIn = false;
            while (!loggedIn)
            {
                try
                {
                    Console.WriteLine("Enter username:");
                    string username = Console.ReadLine().Trim();

                    if (username.Length == 0 || username == "exit")
                    {
                        SendMessageToServer("EXIT", networkHelper);
                        SendMessageToServer("Closing client...\n", networkHelper);
                        break;
                    }

                    Console.WriteLine("Enter password:");
                    string password = Console.ReadLine().Trim();

                    SendMessageToServer(username, networkHelper);
                    SendMessageToServer(password, networkHelper);

                    string response = ReceiveMessageFromServer(networkHelper);

                    if (response == "OK")
                    {
                        Console.WriteLine("Successfully logged in. \n");
                        loggedIn = true;

                        ShowMainMenu(networkHelper);
                    }
                    else if (response == "ERROR")
                    {
                        Console.WriteLine("Incorrect username or password. \n");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(" Error message: " + e.Message);
                }

            }
        }

        private static void ShowMainMenu(NetworkHelper networkHelper)
        {
            Console.WriteLine("Bienvenido al sistema de gestión de Triportunity!");
            Console.WriteLine("¿Qué desea hacer?");
            Console.WriteLine("1-Publicar viaje");
            Console.WriteLine("2-Unirse a un viaje");
            Console.WriteLine("3-Modificar un viaje");
            Console.WriteLine("4-Baja de un viaje");
            Console.WriteLine("5-Buscar un viaje");
            Console.WriteLine("6-Consultar la información de un viaje especifico");
            Console.WriteLine("7-Calificar a un conductor");
            Console.WriteLine("8-Ver calificación de un conductor");
            Console.WriteLine("9-Cerrar Sesion");
            Console.Write("Seleccione una opción: ");

            string res = Console.ReadLine().Trim();

            SendMessageToServer(res, networkHelper);

            switch (res)
            {
                case "2":
                    Console.WriteLine("Ingrese el destino del viaje al que se quiere unir:");
                    string destino = Console.ReadLine().Trim();
                    SendMessageToServer(destino, networkHelper);

                    string lenthOfTripListString= ReceiveMessageFromServer(networkHelper);
                    int lenthOfTripList = int.Parse(lenthOfTripListString);
                    for (int i = 0; i< lenthOfTripList; i++)
                    {
                        string trip = ReceiveMessageFromServer(networkHelper);
                        Console.WriteLine(trip);
                    }

                    break;
                default:
                    SendMessageToServer(res, networkHelper);
                    break;
            }
        }

        private static string ReceiveMessageFromServer(NetworkHelper networkHelper)
        {
            byte[] usernameInBytes = networkHelper.Receive(Protocol.DataLengthSize);
            int usernameLength = BitConverter.ToInt32(usernameInBytes);
            byte[] usernameBufferInBytes = networkHelper.Receive(usernameLength);
            return Encoding.UTF8.GetString(usernameBufferInBytes);
        }

        private static void SendMessageToServer(string message, NetworkHelper networkHelper)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes(message);
            int responseLength = responseBuffer.Length;
            byte[] responseLengthInBytes = BitConverter.GetBytes(responseLength);
            networkHelper.Send(responseLengthInBytes);
            networkHelper.Send(responseBuffer);
        }

        public static void Options()
        {
            Console.WriteLine("Bienvenido al sistema de gestión de Triportunity!");
            bool stay = true;
            do
            {
                Console.WriteLine("¿Qué desea hacer?");
                Console.WriteLine("1-Publicar viaje");
                Console.WriteLine("2-Unirse a un viaje");
                Console.WriteLine("3-Modificar un viaje");
                Console.WriteLine("4-Baja de un viaje");
                Console.WriteLine("5-Buscar un viaje");
                Console.WriteLine("6-Consultar la información de un viaje especifico");
                Console.WriteLine("7-Calificar a un conductor");
                Console.WriteLine("8-Ver calificación de un conductor");
                Console.WriteLine("9-Salir");
                Console.Write("Seleccione una opción: ");

                int option = int.Parse(Console.ReadLine() ?? "0");

                switch (option)
                {
                    case 1:
                        Console.WriteLine("Publicando un viaje...");
                        break;
                    case 2:
                        Console.WriteLine("Uniéndose a un viaje...");
                        break;
                    case 3:
                        Console.WriteLine("Modificando un viaje...");
                        break;
                    case 4:
                        Console.WriteLine("Dando de baja un viaje...");
                        break;
                    case 5:
                        Console.WriteLine("Buscando un viaje...");
                        break;
                    case 6:
                        Console.WriteLine("Consultando información de un viaje...");
                        break;
                    case 7:
                        Console.WriteLine("Calificando a un conductor...");
                        break;
                    case 8:
                        Console.WriteLine("Consultando calificación de un conductor...");
                        break;
                    case 9:
                        stay = false;
                        Console.WriteLine("Saliendo del sistema...");
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                        break;
                }
            } while (stay);
        }
    }
}
