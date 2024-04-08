using Server.BL;

namespace DataAcces
{
    public class CalificationContext
    {
        private static CalificationContext _calificationInstance = null;
        public Dictionary<Guid, Calification> CalificationList = new Dictionary<Guid, Calification>();
        private const string CalificationsFilePath = @"Data\Califications.txt";
        private static Semaphore _calificationSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexCalification = new Semaphore(1, 1);
        private static Semaphore _serviceQueueCalification = new Semaphore(1, 1);
        private static int _readersCalification = 0;

        private CalificationContext()
        {
            LoadCalificationsFromTxt();
        }
        public static CalificationContext GetAccessReadCalification()
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
                _calificationInstance = new CalificationContext();
            return _calificationInstance;
            //fin seccion critica
        }

        public static void ReturnReadAccessCalification()
        {
            _mutexCalification.WaitOne();           //acceso exclusivo al contador de lectores
            _readersCalification--;
            if (_readersCalification == 0)         //si es el ultimo libera a los escritores
                _calificationSemaphore.Release();
            _mutexCalification.Release();           //libera acceso al contador
        }

        public static CalificationContext GetAccessWriteCalification()
        {
            _serviceQueueCalification.WaitOne();
            _calificationSemaphore.WaitOne();
            _serviceQueueCalification.Release();

            //seccion critica
            if (_calificationInstance is null)
                _calificationInstance = new CalificationContext();
            return _calificationInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessCalification()
        {
            _calificationSemaphore.Release();
        }

        private void LoadCalificationsFromTxt()
        {
            throw new NotImplementedException();
        }
    }
}