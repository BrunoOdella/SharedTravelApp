using System.ComponentModel.Design;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common.Interfaces;
using Common;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

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
            Console.WriteLine("9-Cerrar Sesión");
            Console.Write("Seleccione una opción: ");

            string res = Console.ReadLine().Trim();
            switch (int.Parse(res))
            {
                case 1:
                    SendMessageToServer("1", networkHelper);
                    PublishTrip(networkHelper);
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
            byte[] messageInBytes = networkHelper.Receive(Protocol.DataLengthSize);
            int messageLength = BitConverter.ToInt32(messageInBytes);
            byte[] messageBufferInBytes = networkHelper.Receive(messageLength);
            return Encoding.UTF8.GetString(messageBufferInBytes);
        }

        private static void SendMessageToServer(string message, NetworkHelper networkHelper)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes(message);
            int responseLength = responseBuffer.Length;
            byte[] responseLengthInBytes = BitConverter.GetBytes(responseLength);
            networkHelper.Send(responseLengthInBytes);
            networkHelper.Send(responseBuffer);
        }

        private static void PublishTrip(NetworkHelper networkHelper)
        {
            string origin = PromptForNonEmptyString("Introduzca el origen del viaje:");
            SendMessageToServer(origin, networkHelper);

            string destination = PromptForNonEmptyString("Introduzca el destino del viaje:");
            SendMessageToServer(destination, networkHelper);

            DateTime departureDate = PromptForFutureDateTime("Introduzca la fecha y hora de salida (yyyy-mm-dd hh):");
            SendMessageToServer(departureDate.ToString("o"), networkHelper); 

            int availableSeats = PromptForInt("Introduzca el número de asientos disponibles:");
            SendMessageToServer(availableSeats.ToString(), networkHelper);

            float pricePerPassenger = PromptForFloat("Introduzca el precio por pasajero:");
            SendMessageToServer(pricePerPassenger.ToString(), networkHelper);

            bool isPetFriendly = PromptForBoolean("¿Es el viaje amigable con mascotas? (si/no):");
            SendMessageToServer(isPetFriendly.ToString(), networkHelper);

            string photo = Console.ReadLine().Trim();
            SendMessageToServer(photo, networkHelper);


            string response = ReceiveMessageFromServer(networkHelper);
            Console.WriteLine(response);
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
                    }
                }
                else
                {
                    Console.WriteLine("Formato de fecha y hora inválido. Use el formato 'yyyy-MM-dd HH'.");
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
            }
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



    }
}
