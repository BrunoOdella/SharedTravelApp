using Server.BL;
using System.Text.Json;
using System;

namespace DataAcces
{
    public class UserContext
    {
        private static UserContext _userInstance = null;
        public Dictionary<Guid, User> UserList = new Dictionary<Guid, User>();
        private const string UsersFilePath = @"Data\Users.json";
        private static Semaphore _userSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexUser = new Semaphore(1, 1);
        private static Semaphore _serviceQueueUser = new Semaphore(1, 1);
        private static int _readersUser = 0;

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
            return _userInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessUser()
        {
            _userSemaphore.Release();
        }

        public static void LoadUsersFromTxt()
        {
            //cargar los elementos a una lista
            List<UserTransfer> source = new List<UserTransfer>();
            using (StreamReader r = new StreamReader(UsersFilePath))
            {
                string json = r.ReadToEnd();
                source = JsonSerializer.Deserialize<List<UserTransfer>>(json);
            }

            //agregar los elementos de la lista al diccionario
            foreach (var elem in source)
            {
                User actual = new User()
                {
                    Name = elem.nombre,
                    Trips = new List<Guid>()
                };
                Guid actualGuid = new Guid(elem._id);
                actual.SetGuid(actualGuid);
                actual.SetPassword(elem._password);

                _userInstance.UserList.Add(actualGuid, actual);
            }
        }

        public static UserContext CreateInsance()
        {
            _userInstance = new UserContext();
            return _userInstance;
        }
    }

    internal class UserTransfer
    {
        public string nombre { get; set; }
        public string _id { get; set; }
        public string _password { get; set; }
    }
}