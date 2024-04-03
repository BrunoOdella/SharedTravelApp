using System.ComponentModel.Design;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common.Interfaces;
using Common;

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
            soc.Connect(server);

            Console.WriteLine("Cliente conectado con el servidor");

            while (true)
            {
                Console.WriteLine("Ingrese un mensaje para el servidor");
                string message = Console.ReadLine();
                if (String.IsNullOrEmpty(message) || message.Equals("exit"))
                    break;

                byte[] messageInBytes = Encoding.UTF8.GetBytes(message);
                soc.Send(messageInBytes);

            }


            soc.Shutdown(SocketShutdown.Both);
            soc.Close();

            //Menu();
        }

        public static void Menu()
        {
            while (!Autentication())
            {
                Console.WriteLine("Autenticación fallida. Por favor, intente de nuevo.");
            }

            Options();
        }

        public static bool Autentication()
        {
            Console.WriteLine("Inicie sesión para continuar:");
            Console.Write("Ingrese su nombre de usuario: ");
            string user = Console.ReadLine();
            Console.Write("Ingrese su contraseña: ");
            string password = Console.ReadLine();


            return user == "admin" && password == "123";
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
