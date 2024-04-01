using Server.BL;

namespace DataAcces
{

    public class Context
    {
        private static Context _instance = null;
        public Dictionary<Guid, Trip> TripList = new Dictionary<Guid, Trip>();
        public Dictionary<Guid, User> UserList = new Dictionary<Guid, User>();
        public Dictionary<Guid, Calification> CalificationList = new Dictionary<Guid, Calification>();
        private const string TripsFilePath = @"Data\Trips.txt";
        private const string UsersFilePath = @"Data\Users.txt";
        private const string CalificationsFilePath = @"Data\Califications.txt";
        public static Semaphore semaphoreInstance = new Semaphore(1, 1); // parametros: (disponbilesInicial,capacidadMaxima)

        private Context()
        {
            // El constructo es responsable de cargar los datos desde los TXT
            LoadTripsFromTxt();
            LoadUsersFromTxt();
            LoadCalificationsFromTxt();
        }

        public static Context GetInstance()
        {
            semaphoreInstance.WaitOne();
            if (_instance is null)
                _instance = new Context();
            return _instance;
        }

        public static Semaphore GetSemaphore()
        {
            return semaphoreInstance;
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

