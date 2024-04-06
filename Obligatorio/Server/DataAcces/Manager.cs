using Server.BL;

namespace DataAcces
{

    public class Context
    {
        private static Context _tripInstance = null;
        private static Context _userInstance = null;
        private static Context _calificationInstance = null;
        public Dictionary<Guid, Trip> TripList = new Dictionary<Guid, Trip>();
        public Dictionary<Guid, User> UserList = new Dictionary<Guid, User>();
        public Dictionary<Guid, Calification> CalificationList = new Dictionary<Guid, Calification>();
        private const string TripsFilePath = @"Data\Trips.txt";
        private const string UsersFilePath = @"Data\Users.txt";
        private const string CalificationsFilePath = @"Data\Califications.txt";
        private static Semaphore _tripSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexTrip = new Semaphore(1, 1);
        private static Semaphore _userSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexUser = new Semaphore(1, 1);
        private static Semaphore _calificationSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexCalification = new Semaphore(1, 1);
        private static Semaphore _serviceQueueTrip = new Semaphore(1, 1);
        private static Semaphore _serviceQueueUser = new Semaphore(1, 1);
        private static Semaphore _serviceQueueCalification = new Semaphore(1, 1);
        private static int _readersTrip = 0;
        private static int _readersUser = 0;
        private static int _readersCalification = 0;

        // El constructo es responsable de cargar los datos desde los TXT
        private Context()
        {
            LoadTripsFromTxt();
            LoadUsersFromTxt();
            LoadCalificationsFromTxt();
        }

        public static Context GetAccessReadTrip()
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
                _tripInstance = new Context();
            return _tripInstance;
            //fin seccion critica
        }

        public static void ReturnReadAccessTrip()
        {
            _mutexTrip.WaitOne();            //acceso exclusivo al contador de lectores
            _readersTrip--;                 
            if ( _readersTrip == 0)        //si es el ultimo libera a los escritores
                _tripSemaphore.Release();
            _mutexTrip.Release();            //libera acceso al contador
        }

        public static Context GetAccessReadUser()
        {
            _serviceQueueUser.WaitOne();     //espera de turno
            _mutexUser.WaitOne();            //acceso exclusivo al contador de lectores
            _readersUser++;
            if (_readersUser == 1)          //si es el primero bloquea a los escritores
                _userSemaphore.WaitOne();
            _serviceQueueUser.Release();     //libera para el siguiente esperando turno
            _mutexUser.Release();            //libera acceso al contador

            //seccion critica
            if (_userInstance is null)
                _userInstance = new Context();
            return _userInstance;
            //fin seccion critica
        }

        public static void ReturnReadAccessUser()
        {
            _mutexUser.WaitOne();            //acceso exclusivo al contador de lectores
            _readersUser--;
            if (_readersUser == 0)        //si es el ultimo libera a los escritores
                _userSemaphore.Release();
            _mutexUser.Release();            //libera acceso al contador
        }

        public static Context GetAccessReadCalification()
        {
            _serviceQueueCalification.WaitOne();     //espera de turno
            _mutexCalification.WaitOne();            //acceso exclusivo al contador de lectores
            _readersCalification++;
            if (_readersCalification == 1)          //si es el primero bloquea a los escritores
                _calificationSemaphore.WaitOne();
            _serviceQueueCalification.Release();     //libera para el siguiente esperando turno
            _mutexCalification.Release();            //libera acceso al contador

            //seccion critica
            if (_calificationInstance is null)
                _calificationInstance = new Context();
            return _calificationInstance;
            //fin seccion critica
        }

        public static void ReturnReadAccessCalification()
        {
            _mutexCalification.WaitOne();           //acceso exclusivo al contador de lectores
            _readersCalification--;                 
            if ( _readersCalification == 0)         //si es el ultimo libera a los escritores
                _calificationSemaphore.Release();
            _mutexCalification.Release();           //libera acceso al contador
        }

        public static Context GetAccessWriteTrip()
        {
            _serviceQueueTrip.WaitOne();
            _tripSemaphore.WaitOne();
            _serviceQueueTrip.Release();

            //seccion critica
            if (_tripInstance is null)
                _tripInstance = new Context();
            return _tripInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessTrip()
        {
            _tripSemaphore.Release();
        }

        public static Context GetAccessWriteUser()
        {
            _serviceQueueUser.WaitOne();    
            _userSemaphore.WaitOne();
            _serviceQueueUser.Release();

            //seccion critica
            if (_userInstance is null)
                _userInstance = new Context();
            return _userInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessUser()
        {
            _userSemaphore.Release();
        }

        public static Context GetAccessWriteCalification()
        {
            _serviceQueueCalification.WaitOne();
            _calificationSemaphore.WaitOne();
            _serviceQueueCalification.Release();

            //seccion critica
            if (_calificationInstance is null)
                _calificationInstance = new Context();
            return _calificationInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessCalification()
        {
            _calificationSemaphore.Release();
        }

        

        private void LoadTripsFromTxt()
        {
            // Lógica para leer el archivo TXT de Trips y cargarlo en TripList
        }

        private void LoadUsersFromTxt()
        {
            // Lógica para leer el archivo TXT de Users y cargarlo en UserList
        }

        private void LoadCalificationsFromTxt()
        {
            // Lógica para leer el archivo TXT de Califications y cargarlo en CalificationList
        }
    }

}

