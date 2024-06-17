using GrpcServer.Server.BL;
using System.Text.Json;

namespace GrpcServer.Server.DataAcces.Contexts
{
    public class TripContext
    {
        private static TripContext _tripInstance = null;
        public Dictionary<Guid, Trip> TripList = new Dictionary<Guid, Trip>();

        private const string FileName = "Trips.json";
        private static SemaphoreSlim _tripSemaphore = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _mutexTrip = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _serviceQueueTrip = new SemaphoreSlim(1, 1);
        private static int _readersTrip = 0;

        private static string TripsFilePath
        {
            get
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
                return Path.Combine(projectDirectory, "Server", "Data", FileName);
            }
        }

        private static string CarsFilePath
        {
            get
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
                return Path.Combine(projectDirectory, "Server", "Data", "Autos");
            }
        }

        public static async Task<TripContext> GetAccessReadTrip()
        {
            await _serviceQueueTrip.WaitAsync();     //espera de turno
            await _mutexTrip.WaitAsync();            //acceso exclusivo al contador de lectores
            _readersTrip++;
            if (_readersTrip == 1)          //si es el primero bloquea a los escritores
                await _tripSemaphore.WaitAsync();
            _serviceQueueTrip.Release();     //libera para el siguiente esperando turno
            _mutexTrip.Release();            //libera acceso al contador

            //seccion critica
            return _tripInstance;
            //fin seccion critica
        }

        public static async Task ReturnReadAccessTrip()
        {
            await _mutexTrip.WaitAsync();            //acceso exclusivo al contador de lectores
            _readersTrip--;
            if (_readersTrip == 0)        //si es el ultimo libera a los escritores
                _tripSemaphore.Release();
            _mutexTrip.Release();            //libera acceso al contador
        }

        public void LoadTripsFromTxt(UserContext context)
        {
            List<TripTransfer> source = new List<TripTransfer>();
            using (StreamReader r = new StreamReader(TripsFilePath))
            {
                string json = r.ReadToEnd();
                source = JsonSerializer.Deserialize<List<TripTransfer>>(json);
            }
            int count = 1;
            bool scd = false;
            foreach (var elem in source)
            {
                //C:\Users\user\OneDrive - Nublit\Documentos\ORT\prog de redes\Obli\M6A_Ingenieria_242739_231665_256680\Obligatorio\Server\Data\Autos\auto - copia (1).jpg
                //C:\Users\user\OneDrive - Nublit\Documentos\ORT\prog de redes\Obli\M6A_Ingenieria_242739_231665_256680\Obligatorio\Server\Data\Autos\Koopa - copia (1).jpg
                //Path.Combine(parentDirectory.FullName, "Autos");
                string wich;
                if (!scd)
                {
                    wich = $"auto - copia ({count}).jpg";
                    scd = true;
                }
                else
                {
                    wich = $"Koopa - copia ({count}).jpg";
                    scd = false;
                    count++;
                }
                string sourceFile = Path.Combine(CarsFilePath, wich); //Archivo de los datos de prueba

                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string relativePath = "ReceivedFiles";
                string saveDirectory = Path.Combine(basePath, relativePath);

                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                string savePath = Path.Combine(saveDirectory, wich); //Direccion de donde guardar la imagen una vez creado el Trip

                File.Copy(sourceFile, savePath, true); //Copio el archivo de prueba a la pocision donde debe guardarse

                Trip actual = new Trip()
                {
                    Origin = elem.Origen,
                    Destination = elem.Destino,
                    Departure = new DateTime(elem.Anio, elem.Mes, elem.Dia, elem.Hora, 0, 0),
                    AvailableSeats = elem.AsientosDisponibles,
                    TotalSeats = elem.AsientosTotales,
                    PricePerPassanger = elem.Precio,
                    Pet = elem.Mascota,
                    Photo = savePath
                };
                Guid actualGuid = new Guid(elem.TripID);
                actual.SetGuid(actualGuid);
                actual.SetOwner(new Guid(elem.OwnerID));
                List<Guid> passangers = new List<Guid>();
                foreach (var pass in elem.Pasageros)
                {
                    passangers.Add(new Guid(pass));
                    context.UserList[new Guid(pass)].Trips.Add(actualGuid);

                }
                actual.SetPassangers(passangers);

                _tripInstance.TripList.Add(actualGuid, actual);
            }
        }

        public static async Task<TripContext> GetAccessWriteTrip()
        {
            await _serviceQueueTrip.WaitAsync();
            await _tripSemaphore.WaitAsync();
            _serviceQueueTrip.Release();

            //seccion critica
            return _tripInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessTrip()
        {
            _tripSemaphore.Release();
        }

        public static TripContext CreateInsance()
        {
            if (_tripInstance == null)
            {
                _tripInstance = new TripContext();
            }
            return _tripInstance;
        }
    }

    internal class TripTransfer
    {
        public string TripID { get; set; }
        public string OwnerID { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Dia { get; set; }
        public int Hora { get; set; }
        public int AsientosDisponibles { get; set; }
        public int AsientosTotales { get; set; }
        public float Precio { get; set; }
        public bool Mascota { get; set; }
        public string photo { get; set; }
        public string[] Pasageros { get; set; }
    }
}
