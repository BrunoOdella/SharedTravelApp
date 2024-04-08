using Server.BL;

namespace DataAcces
{
    public class UserContext
    {
        private static UserContext _userInstance = null;
        public Dictionary<Guid, User> UserList = new Dictionary<Guid, User>();
        private const string UsersFilePath = @"Data\Users.txt";
        private static Semaphore _userSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexUser = new Semaphore(1, 1);
        private static Semaphore _serviceQueueUser = new Semaphore(1, 1);
        private static int _readersUser = 0;

        private UserContext()
        {
            LoadUsersFromTxt();
        }

        public static UserContext GetAccessReadUser()
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
                _userInstance = new UserContext();
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

        public static UserContext GetAccessWriteUser()
        {
            _serviceQueueUser.WaitOne();
            _userSemaphore.WaitOne();
            _serviceQueueUser.Release();

            //seccion critica
            if (_userInstance is null)
                _userInstance = new UserContext();
            return _userInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessUser()
        {
            _userSemaphore.Release();
        }

        private void LoadUsersFromTxt()
        {
            throw new NotImplementedException();
        }
    }

}