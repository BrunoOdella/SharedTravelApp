using Server.BL;

namespace DataAcces
{

    public class Context
    {
        private static Context _instance = null;
        public Dictionary<Guid, Trip> TripList = new Dictionary<Guid, Trip>();
        public static Semaphore semaphoreInstance = new Semaphore(1, 1); // parametros: (disponbilesInicial,capacidadMaxima)


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
    }

}

