using GrpcServer.Server.BL;
using System.Text.Json;

namespace GrpcServer.Server.DataAcces.Contexts
{
    public class UserContext
    {
        private static UserContext _userInstance = null;
        public Dictionary<Guid, User> UserList = new Dictionary<Guid, User>();
        private const string UsersFileName = "Users.json";
        private static SemaphoreSlim _userSemaphore = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _mutexUser = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _serviceQueueUser = new SemaphoreSlim(1, 1);
        private static int _readersUser = 0;

        private static string UsersFilePath
        {
            get
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
                parentDirectory = Directory.GetParent(parentDirectory.FullName);
                parentDirectory = Directory.GetParent(parentDirectory.FullName);
                return Path.Combine(parentDirectory.FullName, "Data", UsersFileName);
            }
        }

        public static async Task<UserContext> GetAccessReadUser()
        {
            await _serviceQueueUser.WaitAsync();     //espera de turno
            await _mutexUser.WaitAsync();            //acceso exclusivo al contador de lectores
            _readersUser++;
            if (_readersUser == 1)          //si es el primero bloquea a los escritores
                await _userSemaphore.WaitAsync();
            _serviceQueueUser.Release();     //libera para el siguiente esperando turno
            _mutexUser.Release();            //libera acceso al contador

            //seccion critica
            return _userInstance;
            //fin seccion critica
        }

        public static async void ReturnReadAccessUser()
        {
            await _mutexUser.WaitAsync();            //acceso exclusivo al contador de lectores
            _readersUser--;
            if (_readersUser == 0)        //si es el ultimo libera a los escritores
                _userSemaphore.Release();
            _mutexUser.Release();            //libera acceso al contador
        }

        public static async Task<UserContext> GetAccessWriteUser()
        {
            await _serviceQueueUser.WaitAsync();
            await _userSemaphore.WaitAsync();
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
                    Name = elem.Nombre,
                    Trips = new List<Guid>()
                };
                Guid actualGuid = new Guid(elem.Id);
                actual.SetGuid(actualGuid);
                actual.SetPassword(elem.Contrasenia);

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
        public string Nombre { get; set; }
        public string Id { get; set; }
        public string Contrasenia { get; set; }
    }
}
