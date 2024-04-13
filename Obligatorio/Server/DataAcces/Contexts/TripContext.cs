using System.Text.Json;
using Server.BL;

namespace DataAcces
{
    public class TripContext
    {
        private static TripContext _tripInstance = null;
        public Dictionary<Guid, Trip> TripList = new Dictionary<Guid, Trip>();

        private const string TripsFilePath = @"Data\Trips.txt";
        private static Semaphore _tripSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexTrip = new Semaphore(1, 1);
        private static Semaphore _serviceQueueTrip = new Semaphore(1, 1);
        private static int _readersTrip = 0;

        public static TripContext GetAccessReadTrip()
        {
            _serviceQueueTrip.WaitOne();     //espera de turno
            _mutexTrip.WaitOne();            //acceso exclusivo al contador de lectores
            _readersTrip++;
            if (_readersTrip == 1)          //si es el primero bloquea a los escritores
                _tripSemaphore.WaitOne();
            _serviceQueueTrip.Release();     //libera para el siguiente esperando turno
            _mutexTrip.Release();            //libera acceso al contador

            //seccion critica
            return _tripInstance;
            //fin seccion critica
        }

        public static void ReturnReadAccessTrip()
        {
            _mutexTrip.WaitOne();            //acceso exclusivo al contador de lectores
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

            foreach (var elem in source)
            {
                Trip actual = new Trip()
                {
                    Origin = elem.origen,
                    Destination = elem.destino,
                    Departure = new DateTime(elem.anio, elem.mes, elem.dia, elem.hora, 0, 0),
                    AvailableSeats = elem.asientosDisponibles,
                    TotalSeats = elem.asientosDisponibles,
                    PricePerPassanger = elem.precio,
                    Pet = elem.pet,
                    Photo = elem.photo
                };
                Guid actualGuid = new Guid(elem._id);
                actual.SetGuid(actualGuid);
                actual.SetOwner(new Guid(elem._owner));
                List<Guid> passangers = new List<Guid>();
                foreach (var pass in elem._passengers)
                {
                    passangers.Add(new Guid(pass));
                    context.UserList[new Guid(pass)].Trips.Add(actualGuid);

                }
                actual.SetPassangers(passangers);

                _tripInstance.TripList.Add(actualGuid, actual);
            }
        }

        public static TripContext GetAccessWriteTrip()
        {
            _serviceQueueTrip.WaitOne();
            _tripSemaphore.WaitOne();
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
            _tripInstance = new TripContext();
            return _tripInstance;
        }
    }

    internal class TripTransfer
    {
        public string _id { get; set; }
        public string _owner { get; set; }
        public string origen { get; set; }
        public string destino { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
        public int dia { get; set; }
        public int hora { get; set; }
        public int asientosDisponibles { get; set; }
        public int asientosTotales { get; set; }
        public float precio { get; set; }
        public bool pet { get; set; }
        public string photo { get; set; }
        public string[] _passengers { get; set; }
    }
}