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
            switch (int.Parse(res))
            {
                case 3:
                    SendMessageToServer("3", networkHelper);
                    ModifyTrip(networkHelper);
                    break;
                case 9:
                    SendMessageToServer("EXIT", networkHelper);
                    break;
                default:
                    Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
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

        private static void ModifyTrip(NetworkHelper networkHelper)
        {
            //obtener los viajes del user actual que no esten vencidos
            Console.WriteLine("Listado de viajes publicados:");
            string hasTrips = ReceiveMessageFromServer(networkHelper);
            if (hasTrips == "EMPTY")
            {
                Console.WriteLine("No hay viajes publicados para fechas futuras.");
            }
            Console.WriteLine(hasTrips); //verificacion - borrar luego
            //recivo el contador de viajes y muestro uno por uno
            int count = Int32.Parse(hasTrips);
            //muestro los viajes
            for (int i = 0; i < count; i++)
            {
                string currentTrip = ReceiveMessageFromServer(networkHelper);
                Console.WriteLine(currentTrip);
            }
            //fin

            //el user selecciona que viaje o salir
            Console.WriteLine("¿Que viaje desea modificar?\n    (Para volver escriba SALIR)");
            string response = "";
            int wich = -1;
            do
            {
                response = Console.ReadLine().Trim();
            } while (VerifyResponseModifyTrip(count, response, ref wich) || response.Trim().ToLower() == "salir");
            
            if (response.Trim().ToLower() == "salir")
                return;
            //
            //envio que viaje quiero
            SendMessageToServer($"{response}", networkHelper);
            //
            //Que quiero modificar
            Console.WriteLine(ReceiveMessageFromServer(networkHelper)); //viaje a modificar

            String Origin;
            String Destination;
            String DepartureTime;
            String TotalSeats;
            String Pet;
            String Photo;

            Console.WriteLine("Nuevo Origen: (Para no modificar aprete enter)");

            Console.WriteLine("Nuevo Destino: (Para no modificar aprete enter)");

            Console.WriteLine("Nueva fecha y hora de salida (yyyy-mm-dd hh) (Para no modificar aprete enter)");

            Console.WriteLine("Nueva cantidad de asientos disponibles: (Para no modificar aprete enter)");

            Console.WriteLine("¿Es el viaje amigable con mascotas?: (Para no modificar aprete enter)");

            Console.WriteLine("Nuevo precio por pasajero: (Para no modificar aprete enter)");

            Console.WriteLine("Nueva imagen del veiculo: (Para no modificar aprete enter)");

            //Modificacion


            //
        }

        private static bool VerifyResponseModifyTrip(int count, string response, ref int wich)
        {
            return response.Trim().Length == 0 && Int32.TryParse(response, out wich) && wich > 0 && wich < count + 1;
        }
    }
}
