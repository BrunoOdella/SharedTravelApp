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

        private TripContext()
        {
            LoadTripsFromTxt();
        }

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
            if (_tripInstance is null)
                _tripInstance = new TripContext();
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

        private void LoadTripsFromTxt()
        {
            // Lógica para leer el archivo TXT de Trips y cargarlo en TripList
        }

        public static TripContext GetAccessWriteTrip()
        {
            _serviceQueueTrip.WaitOne();
            _tripSemaphore.WaitOne();
            _serviceQueueTrip.Release();

            //seccion critica
            if (_tripInstance is null)
                _tripInstance = new TripContext();
            return _tripInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessTrip()
        {
            _tripSemaphore.Release();
        }

    }
}