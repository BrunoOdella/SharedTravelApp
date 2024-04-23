﻿using System.ComponentModel.Design;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common.Interfaces;
using Common;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;

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
            SendMessageToServer(res, networkHelper);

            switch (int.Parse(res))
            {
                case 1:
                    PublishTrip(networkHelper);
                    break;
                case 2:
                    JoinTrip(networkHelper);
                    break;
                case 3:
                    ModifyTrip(networkHelper);
                    break;
                case 5:
                    TripSearch(networkHelper);
                    break;
                case 7:
                    ViewTripInfo(networkHelper);
                    break;
                case 9:
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

            Console.WriteLine("Escriba la ruta de la foto del coche");
            SendStreamToServer(networkHelper);


            string response = ReceiveMessageFromServer(networkHelper);
            Console.WriteLine(response);
        }


        private static string ReceiveStreamFromServer(NetworkHelper networkHelper, string path)
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

            //string basePath = AppDomain.CurrentDomain.BaseDirectory;
            //string relativePath = "ReceivedFiles";
            string downloadPath = path;

            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            string savePath = Path.Combine(downloadPath, fileName);

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


        private static void ViewTripInfo(NetworkHelper networkHelper)
        {

            string response = ReceiveMessageFromServer(networkHelper);

            if (response.StartsWith("ERROR"))
            {
                Console.WriteLine(response.Substring(5));
                Console.WriteLine();
                ShowMainMenu(networkHelper);
            }
            else
            {
                int tripCount = int.Parse(response);

                if (tripCount > 0)
                {
                    for (int i = 0; i < tripCount; i++)
                    {
                        string trip = ReceiveMessageFromServer(networkHelper);
                        Console.WriteLine(trip);
                    }

                    Console.WriteLine("Ingrese el numero de viaje del que quiere recibir toda la informacion:");
                    string selectedTripNumberStr = Console.ReadLine().Trim();
                    SendMessageToServer(selectedTripNumberStr, networkHelper);

                    string tripInfo = ReceiveMessageFromServer(networkHelper);
                    Console.Write(tripInfo);

                    //le pregunto si desea descargar la imagen del auto
                    Console.WriteLine("¿ Desea descargar la imagen del vehiculo? (si/no)");
                    string resp = Console.ReadLine().Trim();
                    SendMessageToServer(resp, networkHelper);

                    if(resp == "si")
                    {
                       Console.WriteLine("Ingrese el path en el cual desea guardar la imagen");
                       string path = Console.ReadLine().Trim();
                       ReceiveStreamFromServer(networkHelper, path);
                    }



                }
                else
                {
                    Console.WriteLine("No hay viajes disponibles");
                }
                ShowMainMenu(networkHelper);
            }
        }


        private static void SendStreamToServer(NetworkHelper networkHelper)
        {
            string filePath = "";
            bool fileExists = false;

            while (!fileExists)
            {
                Console.WriteLine("Ingrese la ruta del archivo a enviar:");
                filePath = Console.ReadLine();

                if (File.Exists(filePath))
                {
                    fileExists = true;
                }
                else
                {
                    Console.WriteLine("Error: El archivo no existe. Por favor, ingrese una ruta válida.");
                }
            }

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
                int numberOfBytesToSend = isLastPart ? (int)(fileLength - offset) : Protocol.MaxPartSize;
                Console.WriteLine($"Enviando parte #{currentPart}, de {numberOfBytesToSend} bytes");

                byte[] bytesReadFromDisk = fs.Read(filePath, offset, numberOfBytesToSend);
                networkHelper.Send(bytesReadFromDisk);
                currentPart++;
                offset += numberOfBytesToSend;
            }
            Console.WriteLine($"Terminé de enviar archivo {filePath}, de tamaño {fileLength} bytes");
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
                Int32.TryParse(response, out wich);
                bool a = (response.Trim().Length == 0 || wich < 0 || wich > count + 1);
                bool b = response.Trim().ToLower() == "salir";
            } while ((response.Trim().Length == 0 || wich < 0 || wich > count + 1) || response.Trim().ToLower() == "salir");

            //response.Trim().Length != 0 && Int32.TryParse(response, out wich) && wich > 0 && wich < count + 1;

            if (response.Trim().ToLower() == "salir")
                return;
            //
            //envio que viaje quiero
            SendMessageToServer($"{response}", networkHelper);
            //
            //Que quiero modificar
            Console.WriteLine(ReceiveMessageFromServer(networkHelper)); //viaje a modificar

            bool modificar;
            String Origin;
            String Destination;
            DateTime DepartureTime;
            int PricePerSeat;
            bool Pet;
            String Photo;


            modificar = PromptForBoolean("¿Modificar Origen? (SI/NO)");
            if (modificar)
            {
                Origin = PromptForNonEmptyString("Nuevo Origen:");
                SendMessageToServer(Origin, networkHelper);
            }
            else SendMessageToServer("EMPTY", networkHelper);

            modificar = PromptForBoolean("¿Modificar Destino? (SI/NO)");
            if (modificar)
            {
                Destination = PromptForNonEmptyString("Nuevo Destino:");
                SendMessageToServer(Destination, networkHelper);
            }
            else SendMessageToServer("EMPTY", networkHelper);

            modificar = PromptForBoolean("¿Modificar fecha y hora de salida? (SI/NO)");
            if (modificar)
            {
                DepartureTime = PromptForFutureDateTime("Nueva fecha y hora de salida (yyyy-mm-dd hh):");
                SendMessageToServer(DepartureTime.ToString("o"), networkHelper);
            }
            else SendMessageToServer("EMPTY", networkHelper);

            modificar = PromptForBoolean("¿Modificar si el viaje es amigable con mascotas? (SI/NO)");
            if (modificar)
            {
                Pet = PromptForBoolean("¿Es el viaje amigable con mascotas?:");
                SendMessageToServer(Pet.ToString(), networkHelper);
            }
            else SendMessageToServer("EMPTY", networkHelper);

            modificar = PromptForBoolean("¿Modificar el precio por pasajero? (SI/NO)");
            if (modificar)
            {
                PricePerSeat = PromptForInt("Nuevo precio por pasajero:");
                SendMessageToServer(PricePerSeat.ToString(), networkHelper);
            }
            else SendMessageToServer("EMPTY", networkHelper);

            modificar = PromptForBoolean("¿Modificar la imagen del veiculo? (SI/NO)");
            if (modificar)
            {
                SendMessageToServer(modificar.ToString(), networkHelper); //avisarle al server que va recibir una foto
                SendStreamToServer(networkHelper);
            }
            else
            {
                SendMessageToServer(modificar.ToString(), networkHelper);
                SendMessageToServer("EMPTY", networkHelper);
            }

            //Modificacion
            //RECIBIR UN MODIFICADO O ERROR
            Console.WriteLine(ReceiveMessageFromServer(networkHelper));
            //FIN TOTAL
        }

        private static void JoinTrip(NetworkHelper networkHelper)
        {
            Console.WriteLine("Ingrese el origen del viaje al que se quiere unir:");
            string origin = Console.ReadLine().Trim();

            Console.WriteLine("Ingrese el destino del viaje al que se quiere unir:");
            string destination = Console.ReadLine().Trim();

            SendMessageToServer(origin, networkHelper);
            SendMessageToServer(destination, networkHelper);

            string response = ReceiveMessageFromServer(networkHelper);

            if (response.StartsWith("ERROR"))
            {
                Console.WriteLine(response.Substring(5)); 
                Console.WriteLine();
                ShowMainMenu(networkHelper);
            }
            else
            {
                int tripCount = int.Parse(response);

                if (tripCount > 0)
                {
                    for (int i = 0; i < tripCount; i++)
                    {
                        string trip = ReceiveMessageFromServer(networkHelper);
                        Console.WriteLine(trip);
                    }

                    Console.WriteLine("Ingrese el número del viaje al que desea unirse:");
                    string selectedTripNumberStr = Console.ReadLine().Trim();
                    SendMessageToServer(selectedTripNumberStr, networkHelper);

                    Console.WriteLine("Se ha unido correctamente al viaje");
                    ShowMainMenu(networkHelper);
                }
                else
                {
                    Console.WriteLine("No hay viajes disponibles para el origen y destino especificado.");
                    ShowMainMenu(networkHelper);
                }
            }
        }
         private static void TripSearch (NetworkHelper networkHelper)
        {
            Console.WriteLine("Desea:");
            Console.WriteLine("1. Ver la lista de todos los viajes");
            Console.WriteLine("2. Ver la lista de viajes ingrsando el origen y destino deseados");

            string res = Console.ReadLine().Trim();
            SendMessageToServer(res, networkHelper);

            switch (int.Parse(res))
            {
                case 1:
                    ViewAllTrips(networkHelper);
                    break;
                case 2:
                    ViewTripsOriginDestination(networkHelper);
                    break;
                default:
                    Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                    break;
            }
        }

        
        private static void ViewAllTrips(NetworkHelper networkHelper)
        {
            string response = ReceiveMessageFromServer(networkHelper);
            int tripCount = int.Parse(response);

            Console.WriteLine();
            if (tripCount > 0)
            {
                for (int i = 0; i < tripCount; i++)
                {
                    string trip = ReceiveMessageFromServer(networkHelper);
                    Console.WriteLine(trip);
                }
                Console.WriteLine();
                Console.WriteLine();
                
            }
            else
            {
                Console.WriteLine("No hay viajes disponibles");
                
            }
            ShowMainMenu(networkHelper);
        }

        private static void ViewTripsOriginDestination(NetworkHelper networkHelper)
        {
            Console.WriteLine("Ingrese el origen del viaje al que se quiere unir:");
            string origin = Console.ReadLine().Trim();

            Console.WriteLine("Ingrese el destino del viaje al que se quiere unir:");
            string destination = Console.ReadLine().Trim();

            SendMessageToServer(origin, networkHelper);
            SendMessageToServer(destination, networkHelper);

            string response = ReceiveMessageFromServer(networkHelper);

            if (response.StartsWith("ERROR"))
            {
                Console.WriteLine(response.Substring(5));
                Console.WriteLine();
               
            }
            else
            {
                Console.WriteLine();
                int tripCount = int.Parse(response);

                if (tripCount > 0)
                {
                    for (int i = 0; i < tripCount; i++)
                    {
                        string trip = ReceiveMessageFromServer(networkHelper);
                        Console.WriteLine(trip);
                    }
                }
                else
                {
                    Console.WriteLine("No hay viajes disponibles para el origen y destino especificado.");
                    
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            ShowMainMenu(networkHelper);
        }




        private static bool VerifyResponseModifyTrip(int count, string response, ref int wich)
        {
            return response.Trim().Length != 0 && Int32.TryParse(response, out wich) && wich > 0 && wich < count + 1;
        }
    }
}
