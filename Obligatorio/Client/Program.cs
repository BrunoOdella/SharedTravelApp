using System.ComponentModel.Design;
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
        static CancellationTokenSource token = new CancellationTokenSource();
        static async Task Main(string[] args)
        {
            IPEndPoint local = new IPEndPoint(
                IPAddress.Parse(SettingsMgr.ReadSetting(ClientConfig.ClientIpConfigKey)),
                int.Parse(SettingsMgr.ReadSetting(ClientConfig.ClientPortConfigKey))
            );
            IPEndPoint server = new IPEndPoint(
                IPAddress.Parse(SettingsMgr.ReadSetting(ClientConfig.ServerIpConfigKey)),
                int.Parse(SettingsMgr.ReadSetting(ClientConfig.SeverPortConfigKey))
            );

            TcpClient client = new TcpClient();

            try
            {
                client.Client.Bind(local);
                await client.ConnectAsync(server);

                Console.WriteLine("Cliente conectado con el servidor");
                await LogInAsync(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.ReadLine();
            }
            finally
            {
                client.Close();
            }
        }

        private static async Task LogInAsync(TcpClient client)
        {
            NetworkHelper networkHelper = new NetworkHelper(client);
            bool loggedIn = false;
            while (!loggedIn)
            {
                try
                {
                    Console.WriteLine("Enter username:");
                    string username = Console.ReadLine().Trim();

                    if (username.Length == 0 || username == "exit")
                    {
                        await SendMessageToServerAsync("EXIT", networkHelper, token.Token);
                        await SendMessageToServerAsync("Closing client...\n", networkHelper, token.Token);
                        break;
                    }

                    Console.WriteLine("Enter password:");
                    string password = Console.ReadLine().Trim();

                    await SendMessageToServerAsync(username, networkHelper, token.Token);
                    await SendMessageToServerAsync(password, networkHelper, token.Token);

                    string response = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

                    if (response == "OK")
                    {
                        Console.WriteLine("Successfully logged in. \n");
                        loggedIn = true;

                        Console.Title = username.ToUpper();

                        Console.Clear();

                        await ShowMainMenu(networkHelper);
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

        private static async Task ShowMainMenu(NetworkHelper networkHelper)
        {
            bool logout = false;
            while (!logout)
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
                Console.WriteLine("9-Eliminar viaje");
                Console.WriteLine("10-Cerrar Sesión");
                Console.Write("Seleccione una opción: ");

                string res = "";
                do
                {
                    res = Console.ReadLine().Trim();
                } while (res.Length == 0 || int.Parse(res) < 0 || int.Parse(res) > 10);

                await SendMessageToServerAsync(res, networkHelper, token.Token);
                Console.Clear();

                switch (int.Parse(res))
                {
                    case 1:
                        await PublishTripAsync(networkHelper);
                        break;
                    case 2:
                        await JoinTripAsync(networkHelper);
                        break;
                    case 3:
                        await ModifyTripAsync(networkHelper);
                        break;
                    case 4:
                        await WithdrawFromTripAsync(networkHelper);
                        break;
                    case 5:
                        await TripSearchAsync(networkHelper);
                        break;
                    case 6:
                        await ViewTripInfoAsync(networkHelper);
                        break;
                    case 7:
                        await RateDriverAsync(networkHelper);
                        break;
                    case 8:
                        await ViewDriverRatingsAsync(networkHelper);
                        break;
                    case 9:
                        await DeleteTripAsync(networkHelper);
                        break;
                    case 10:
                        logout = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                        break;
                }
            }
        }

        private static async Task DeleteTripAsync(NetworkHelper networkHelper)
        {
            string TripCount = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            if (TripCount == "0")
            {
                Console.WriteLine();
                Console.WriteLine("No hay viajes futuros.");
                Console.WriteLine();
                return;
            }

            int count = Int32.Parse(TripCount);

            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(await ReceiveMessageFromServerAsync(networkHelper, token.Token));
            }

            Console.WriteLine("¿Que viaje desea dar de baja?\n    (Para volver escriba SALIR)");

            string response = "";
            int wich = -1;
            do
            {
                response = Console.ReadLine().Trim();
                Int32.TryParse(response, out wich);
            } while ((response.Trim().Length == 0 || wich < 0 || wich > count) && response.Trim().ToLower() != "salir");

            if (response.Trim().ToLower() == "salir")
            {
                await SendMessageToServerAsync($"{response.ToLower()}", networkHelper, token.Token);
                return;
            }

            await SendMessageToServerAsync($"{wich}", networkHelper, token.Token);

            string ServerResponse = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            Console.Clear();

            Console.WriteLine(ServerResponse);
        }


        private static async Task WithdrawFromTripAsync(NetworkHelper networkHelper)
        {
            string TripCount = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            if (TripCount == "EMPTY")
            {
                Console.WriteLine();
                Console.WriteLine("No hay viajes futuros.");
                Console.WriteLine();
                return;
            }

            int count = Int32.Parse(TripCount);
            for (int i = 0; i < count; i++)
            {
                string currentTrip = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
                Console.WriteLine(currentTrip);
            }

            Console.WriteLine("¿De que viaje desea darse de baja?\n    (Para volver escriba SALIR)");
            string response = "";
            int wich = -1;
            do
            {
                response = Console.ReadLine().Trim();
                Int32.TryParse(response, out wich);
            } while ((response.Trim().Length == 0 || wich < 0 || wich > count) && response.Trim().ToLower() != "salir");

            if (response.Trim().ToLower() == "salir")
            {
                await SendMessageToServerAsync($"{response.ToLower()}", networkHelper, token.Token);
                return;
            }
            //
            //envio que viaje quiero
            await SendMessageToServerAsync($"{wich}", networkHelper, token.Token);

            string ServerResponse = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            if (ServerResponse == "OK")
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("Se a dado de baja del viaje.");
                Console.WriteLine();
            }
        }
        private static async Task RateDriverAsync(NetworkHelper networkHelper)
        {
            Console.WriteLine("Seleccione que conductor calificar:");

            string hasTrips = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            if (hasTrips == "EMPTY")
            {
                Console.WriteLine();
                Console.WriteLine("No se han realizado viajes.");
                Console.WriteLine();
                return;
            }

            int count = Int32.Parse(hasTrips);
            for (int i = 0; i < count; i++)
            {
                string current = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
                Console.WriteLine(current);
            }

            Console.WriteLine("¿A que conductor de que viaje desea calificar?\n    (Para volver escriba SALIR)");
            string response = "";
            int wich = -1;
            do
            {
                response = Console.ReadLine().Trim();
                Int32.TryParse(response, out wich);
                bool a = response.Trim().ToLower() == "salir";
            } while ((response.Trim().Length == 0 || wich < 0 || wich > count) && response.Trim().ToLower() != "salir"); //dejar igual en todos los lugares

            if (response.Trim().ToLower() == "salir")
            {
                await SendMessageToServerAsync($"{response.ToLower()}", networkHelper, token.Token);
                return;
            }


            await SendMessageToServerAsync($"{response}", networkHelper, token.Token);

            float score = PromptForFloat("Introduzca el puntaje (0.0 - 10.0):");
            await SendMessageToServerAsync(score.ToString(), networkHelper, token.Token);

            string comment = PromptForNonEmptyString("Introduzca un comentario:");
            await SendMessageToServerAsync(comment, networkHelper, token.Token);

            Console.WriteLine(await ReceiveMessageFromServerAsync(networkHelper, token.Token));
        }

        private static async Task<string> ReceiveMessageFromServerAsync(NetworkHelper networkHelper, CancellationToken token)
        {
            byte[] messageInBytes = await networkHelper.ReceiveAsync(Protocol.DataLengthSize, token);
            int messageLength = BitConverter.ToInt32(messageInBytes);
            byte[] messageBufferInBytes = await networkHelper.ReceiveAsync(messageLength, token);
            return Encoding.UTF8.GetString(messageBufferInBytes);
        }

        private static async Task SendMessageToServerAsync(string message, NetworkHelper networkHelper, CancellationToken token)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes(message);
            int responseLength = responseBuffer.Length;
            byte[] responseLengthInBytes = BitConverter.GetBytes(responseLength);
            await networkHelper.SendAsync(responseLengthInBytes, token);
            await networkHelper.SendAsync(responseBuffer, token);
        }


        private static async Task PublishTripAsync(NetworkHelper networkHelper)
        {
            string origin = PromptForNonEmptyString("Introduzca el origen del viaje:");
            await SendMessageToServerAsync(origin, networkHelper, token.Token);

            string destination = PromptForNonEmptyString("Introduzca el destino del viaje:");
            await SendMessageToServerAsync(destination, networkHelper, token.Token);

            DateTime departureDate = PromptForFutureDateTime("Introduzca la fecha y hora de salida (yyyy-mm-dd hh):");
            await SendMessageToServerAsync(departureDate.ToString("o"), networkHelper, token.Token); 

            int availableSeats = PromptForInt("Introduzca el número de asientos disponibles:");
            await SendMessageToServerAsync(availableSeats.ToString(), networkHelper, token.Token);

            float pricePerPassenger = PromptForFloat("Introduzca el precio por pasajero:");
            await SendMessageToServerAsync(pricePerPassenger.ToString(), networkHelper, token.Token);

            bool isPetFriendly = PromptForBoolean("¿Es el viaje amigable con mascotas? (si/no):");
            await SendMessageToServerAsync(isPetFriendly.ToString(), networkHelper, token.Token);

            Console.WriteLine("Escriba la ruta de la foto del coche");
            await SendStreamToServerAsync(networkHelper);

            Console.Clear();

            string response = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            Console.WriteLine(response);
        }


        private static async Task<string> ReceiveStreamFromServerAsync(NetworkHelper networkHelper, string path)
        {
            byte[] fileNameLengthInBytes = await networkHelper.ReceiveAsync(Protocol.fileNameLengthSize, token.Token);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthInBytes);

            byte[] fileNameInBytes = await networkHelper.ReceiveAsync(fileNameLength, token.Token);
            string fileName = Encoding.UTF8.GetString(fileNameInBytes);

            byte[] fileLengthInBytes = await networkHelper.ReceiveAsync(Protocol.fileSizeLength, token.Token);
            long fileLength = BitConverter.ToInt64(fileLengthInBytes);

            long numberOfParts = Protocol.numberOfParts(fileLength);

            int currentPart = 1;
            int offset = 0;

            string relativePath = "ReceivedFiles";
            string saveDirectory = Path.Combine(path, relativePath);

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

                byte[] buffer = await networkHelper.ReceiveAsync(numberOfBytesToReceive, token.Token);

                await fs.WriteAsync(savePath, buffer);

                currentPart++;
                offset += numberOfBytesToReceive;
            }
            Console.WriteLine();
            Console.WriteLine($"Archivo recibido completamente y guardado en {savePath}, tamaño total {fileLength} bytes");
            Console.WriteLine();

            return savePath;
        }




        private static async Task ViewTripInfoAsync(NetworkHelper networkHelper)
        {
            await TripSearchAsync(networkHelper);
            //string response = ReceiveMessageFromServerAsync(networkHelper);
            string amountOfTrips= await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            int tripCount = Convert.ToInt16(amountOfTrips);
            if (tripCount != 0) {

                Console.WriteLine("Ingrese el numero de viaje del que quiere recibir toda la informacion:");
                string response = Console.ReadLine().Trim();
                int selectedTripNumber;

                while (!int.TryParse(response, out selectedTripNumber) || selectedTripNumber < 1 || selectedTripNumber > tripCount)
                {
                    Console.WriteLine("Ingrese nuevamente el número de viaje:");
                    response = Console.ReadLine().Trim();
                }

                Console.Clear();

                await SendMessageToServerAsync(response, networkHelper, token.Token);

                await RecevieAllTripInfo(networkHelper);

                Console.WriteLine("¿ Desea descargar la imagen del vehiculo? (si/no)");
                string resp = Console.ReadLine().Trim();

                while (resp.ToLower() != "si" && resp.ToLower() != "no")
                {
                    Console.WriteLine("Ingrese nuevamente la respuesta:");
                    resp = Console.ReadLine().Trim();
                }
                await SendMessageToServerAsync(resp, networkHelper, token.Token);

                if (resp == "si")
                {
                    string path = "";
                    bool pathExists = false;
                    while (!pathExists)
                    {
                        Console.WriteLine("Ingrese la ruta del directorio en el cual desea descargar la imagen:");
                        path = Console.ReadLine().Trim();

                        if (Directory.Exists(path))
                        {
                            pathExists = true;
                        }
                        else
                        {
                            Console.WriteLine("Error: El directorio no existe. Por favor, ingrese una ruta válida.");
                        }
                    }
                    await ReceiveStreamFromServerAsync(networkHelper, path);
                }
            }
            
  
        }


        private static async Task SendStreamToServerAsync(NetworkHelper networkHelper)
        {
            string filePath = "";
            bool fileExists = false;

            while (!fileExists)
            {
                Console.WriteLine("Ingrese la ruta del archivo a enviar (sin comillas por favor):");
                filePath = Console.ReadLine().Trim();

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
            await networkHelper.SendAsync(fileNameLengthInBytes, token.Token);

            await networkHelper.SendAsync(fileNameInBytes, token.Token);

            long fileLength = fileInfo.Length;
            byte[] fileLengthInBytes = BitConverter.GetBytes(fileLength);
            await networkHelper.SendAsync(fileLengthInBytes, token.Token);

            long numberOfParts = Protocol.numberOfParts(fileLength);
            int currentPart = 1;
            int offset = 0;

            FileStreamHelper fs = new FileStreamHelper();
            while (offset < fileLength)
            {
                bool isLastPart = (currentPart == numberOfParts);
                int numberOfBytesToSend = isLastPart ? (int)(fileLength - offset) : Protocol.MaxPartSize;

                byte[] bytesReadFromDisk = await fs.ReadAsync(filePath, offset, numberOfBytesToSend);
                await networkHelper.SendAsync(bytesReadFromDisk, token.Token);
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

        private static async Task ModifyTripAsync(NetworkHelper networkHelper)
        {
            //obtener los viajes del user actual que no esten vencidos
            Console.WriteLine("Listado de viajes publicados:");

            string hasTrips = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            if (hasTrips == "EMPTY")
            {
                Console.WriteLine();
                Console.WriteLine("No hay viajes publicados para fechas futuras.");
                Console.WriteLine();
                return;
            }
            Console.WriteLine(hasTrips); //verificacion - borrar luego
            //recivo el contador de viajes y muestro uno por uno
            int count = Int32.Parse(hasTrips);
            //muestro los viajes
            for (int i = 0; i < count; i++)
            {
                string currentTrip = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
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
            } while ((response.Trim().Length == 0 || wich < 0 || wich > count) && response.Trim().ToLower() != "salir");

            if (response.Trim().ToLower() == "salir")
            {
                await SendMessageToServerAsync("null", networkHelper, token.Token);
                return;
            }
                
            //envio que viaje quiero
            await SendMessageToServerAsync($"{response}", networkHelper, token.Token);
            //Que quiero modificar
            Console.WriteLine(ReceiveMessageFromServerAsync(networkHelper, token.Token)); //viaje a modificar

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
                await SendMessageToServerAsync(Origin, networkHelper, token.Token);
            }
            else await SendMessageToServerAsync("EMPTY", networkHelper, token.Token);

            modificar = PromptForBoolean("¿Modificar Destino? (SI/NO)");
            if (modificar)
            {
                Destination = PromptForNonEmptyString("Nuevo Destino:");
                await SendMessageToServerAsync(Destination, networkHelper, token.Token);
            }
            else await SendMessageToServerAsync("EMPTY", networkHelper, token.Token);

            modificar = PromptForBoolean("¿Modificar fecha y hora de salida? (SI/NO)");
            if (modificar)
            {
                DepartureTime = PromptForFutureDateTime("Nueva fecha y hora de salida (yyyy-mm-dd hh):");
                await SendMessageToServerAsync(DepartureTime.ToString("o"), networkHelper, token.Token);
            }
            else await SendMessageToServerAsync("EMPTY", networkHelper, token.Token);

            modificar = PromptForBoolean("¿Modificar si el viaje es amigable con mascotas? (SI/NO)");
            if (modificar)
            {
                Pet = PromptForBoolean("¿Es el viaje amigable con mascotas?:");
                await SendMessageToServerAsync(Pet.ToString(), networkHelper, token.Token);
            }
            else await SendMessageToServerAsync("EMPTY", networkHelper, token.Token);

            modificar = PromptForBoolean("¿Modificar el precio por pasajero? (SI/NO)");
            if (modificar)
            {
                PricePerSeat = PromptForInt("Nuevo precio por pasajero:");
                await SendMessageToServerAsync(PricePerSeat.ToString(), networkHelper, token.Token);
            }
            else await SendMessageToServerAsync("EMPTY", networkHelper, token.Token);

            modificar = PromptForBoolean("¿Modificar la imagen del veiculo? (SI/NO)");
            if (modificar)
            {
                await SendMessageToServerAsync(modificar.ToString(), networkHelper, token.Token); //avisarle al server que va recibir una foto
                await SendStreamToServerAsync(networkHelper);
            }
            else
            {
                await SendMessageToServerAsync(modificar.ToString(), networkHelper, token.Token);
            }

            //Modificacion
            //RECIBIR UN MODIFICADO O ERROR
            Console.WriteLine(await ReceiveMessageFromServerAsync(networkHelper, token.Token));
            //FIN TOTAL
        }

        private static async Task JoinTripAsync(NetworkHelper networkHelper)
        {
            await ViewAllFutureTripsAsync(networkHelper);
            string amountOfTrips= await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            if (amountOfTrips != "0")
            {
                Console.WriteLine("Ingrese el número del viaje al que desea unirse:");
                Console.WriteLine("Si desea salir ingrese 'salir'");
                string response = Console.ReadLine().Trim();
                int selectedTripNumber;

                if (response.ToLower() == "salir")
                {
                    await SendMessageToServerAsync("exit", networkHelper, token.Token);
                    return;
                }
                while (!int.TryParse(response, out selectedTripNumber) || selectedTripNumber < 1 || selectedTripNumber > Convert.ToInt32(amountOfTrips))
                {
                    Console.WriteLine("Ingrese nuevamente el número de viaje:");
                    response = Console.ReadLine().Trim();
                }
                await SendMessageToServerAsync(response, networkHelper, token.Token);

                Console.Clear();

                string resp = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
                Console.WriteLine(resp);
            }
            
            
        }
         private static async Task TripSearchAsync (NetworkHelper networkHelper)
        {
            Console.Clear();

            Console.WriteLine("Desea:");
            Console.WriteLine("1. Ver la lista de todos los viajes");
            Console.WriteLine("2. Ver la lista de viajes filtrados por origen y destino");
            Console.WriteLine("3. Ver la lista de viajes filtrados por si son pet friendly o no");
            Console.WriteLine("4. Ver la lista de futuros viajes");

            string res = Console.ReadLine().Trim();
            int selectedOpt;

            while (!int.TryParse(res, out selectedOpt) || selectedOpt < 0 || selectedOpt > 4)
            {
                Console.WriteLine("Ingrese nuevamente la opcion:");
                res = Console.ReadLine().Trim();
            }
            await SendMessageToServerAsync(res, networkHelper, token.Token);
            
            Console.Clear();

            switch (int.Parse(res))
            {
                case 1:
                    await ViewAllTripsAsync(networkHelper);
                    break;
                case 2:
                    await ViewTripsOriginDestinationAsync(networkHelper);
                    break;
                case 3:
                    await ViewAllTripsFilteredPetFriendlyAsync(networkHelper);
                    break;
                case 4:
                    await ViewAllFutureTripsAsync(networkHelper);
                    break;
                default:
                    Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                    break;
            }
        }

        private static async Task ViewAllFutureTripsAsync(NetworkHelper networkHelper)
        {
            string response = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            int tripCount = int.Parse(response);

            Console.WriteLine();
            if (tripCount > 0)
            {
                for (int i = 0; i < tripCount; i++)
                {
                    string trip = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
                    Console.WriteLine(trip);
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("No hay viajes disponibles");
                Console.WriteLine();
            }
        }

        private static async Task ViewAllTripsFilteredPetFriendlyAsync(NetworkHelper networkHelper)
        {
            Console.WriteLine("Ingrese 'SI' si desea ver los viajes pet friendly o ingrese 'NO' si desea ver los viajes que no son pet friendly: ");
            string resp = Console.ReadLine().Trim();

            while (resp.ToLower() != "si" && resp.ToLower() != "no")
            {
                Console.WriteLine("Ingrese nuevamente la respuesta:");
                resp = Console.ReadLine().Trim();
            }

            await SendMessageToServerAsync(resp, networkHelper, token.Token);

            string response = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            Console.Clear();

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
                        string trip = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
                        Console.WriteLine(trip);
                    }
                }
                else
                {
                    Console.WriteLine("No hay viajes disponibles para el filtro ingresado");

                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static async Task ViewAllTripsAsync(NetworkHelper networkHelper)
        {
            string response = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            int tripCount = int.Parse(response);

            Console.Clear();

            Console.WriteLine();
            if (tripCount > 0)
            {
                for (int i = 0; i < tripCount; i++)
                {
                    string trip = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
                    Console.WriteLine(trip);
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("No hay viajes disponibles");
            }
        }

        private static async Task ViewTripsOriginDestinationAsync(NetworkHelper networkHelper)
        {
            Console.WriteLine("Ingrese el origen del viaje:");
            string origin = Console.ReadLine().Trim();

            Console.WriteLine("Ingrese el destino del viaje:");
            string destination = Console.ReadLine().Trim();

            await SendMessageToServerAsync(origin, networkHelper, token.Token);
            await SendMessageToServerAsync(destination, networkHelper, token.Token);

            string response = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            if (response.StartsWith("ERROR"))
            {
                Console.WriteLine(response.Substring(5));
                Console.WriteLine();
               
            }
            else
            {
                Console.WriteLine();
                int tripCount = int.Parse(response);

                Console.Clear();

                if (tripCount > 0)
                {
                    for (int i = 0; i < tripCount; i++)
                    {
                        string trip = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
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
        }

        private static async Task RecevieAllTripInfo(NetworkHelper networkHelper)
        {
            string origin= await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            string destination = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            string departure = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            string availableSeats = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            string pricePerPassanger = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            string pet = await ReceiveMessageFromServerAsync(networkHelper, token.Token);

            Console.WriteLine("Origen: " + origin);
            Console.WriteLine("Destino: " + destination);
            Console.WriteLine("Fecha y hora de salida: "+ departure);
            Console.WriteLine("Cantidad de asientos disponibles: " + availableSeats);
            Console.WriteLine("Precio del pasaje por persona: $" + pricePerPassanger);
            if(pet == "true")
            {
                Console.WriteLine("Es pet friendly");
            }
            else
            {
                Console.WriteLine("No es pet friendly");
            }
        }

        private static bool VerifyResponseModifyTripAsync(int count, string response, ref int wich)
        {
            return response.Trim().Length != 0 && Int32.TryParse(response, out wich) && wich > 0 && wich < count + 1;
        }

        private static async Task ViewDriverRatingsAsync(NetworkHelper networkHelper)
        {
            Console.WriteLine("Usuarios disponibles en el sistema:");
            string userNames = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            Console.WriteLine(userNames);

            Console.WriteLine("Ingrese el nombre del usuario para ver sus calificaciones:");
            string selectedUsername = Console.ReadLine();
            await SendMessageToServerAsync(selectedUsername, networkHelper, token.Token);

            Console.Clear();

            string response = await ReceiveMessageFromServerAsync(networkHelper, token.Token);
            Console.WriteLine("Calificaciones recibidas del servidor:");
            Console.WriteLine(response);
        }
    }
}
