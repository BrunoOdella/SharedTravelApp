using GrpcServer.Server.BL;
using System.Text.Json;

namespace GrpcServer.Server.DataAcces.Contexts
{
    public class CalificationContext
    {
        private static CalificationContext _calificationInstance = null;
        public Dictionary<Guid, Calification> CalificationList = new Dictionary<Guid, Calification>();
        private const string FileName = "Califications.json";
        private static SemaphoreSlim _calificationSemaphore = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _mutexCalification = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _serviceQueueCalification = new SemaphoreSlim(1, 1);
        private static int _readersCalification = 0;

        private static string CalificationsFilePath
        {
            get
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
                return Path.Combine(projectDirectory, "Server", "Data", FileName);
            }
        }

        public static async Task<CalificationContext> GetAccessReadCalification()
        {
            await _serviceQueueCalification.WaitAsync();     //espera de turno
            await _mutexCalification.WaitAsync();            //acceso exclusivo al contador de lectores
            _readersCalification++;
            if (_readersCalification == 1)          //si es el primero bloquea a los escritores
                await _calificationSemaphore.WaitAsync();
            _serviceQueueCalification.Release();     //libera para el siguiente esperando turno
            _mutexCalification.Release();            //libera acceso al contador

            //seccion critica
            return _calificationInstance;
            //fin seccion critica
        }

        public static async void ReturnReadAccessCalification()
        {
            await _mutexCalification.WaitAsync();           //acceso exclusivo al contador de lectores
            _readersCalification--;
            if (_readersCalification == 0)         //si es el ultimo libera a los escritores
                _calificationSemaphore.Release();
            _mutexCalification.Release();           //libera acceso al contador
        }

        public static async Task<CalificationContext> GetAccessWriteCalification()
        {
            await _serviceQueueCalification.WaitAsync();
            await _calificationSemaphore.WaitAsync();
            _serviceQueueCalification.Release();

            //seccion critica
            return _calificationInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessCalification()
        {
            _calificationSemaphore.Release();
        }

        public static void LoadCalificationsFromTxt(UserContext userContenxt, TripContext tripContext)
        {
            List<CalificationTransfer> source = new List<CalificationTransfer>();
            using (StreamReader r = new StreamReader(CalificationsFilePath))
            {
                string json = r.ReadToEnd();
                source = JsonSerializer.Deserialize<List<CalificationTransfer>>(json);
            }

            foreach (var elem in source)
            {
                Guid guidActual = new Guid(elem.CalificationID);
                Guid trip = new Guid(elem.TripID);
                Calification actual = new Calification(new Guid(elem.PasageroID), trip, elem.Calificacion, elem.Comentario);
                actual.SetGuid(guidActual);
                _calificationInstance.CalificationList.Add(guidActual, actual);
                Guid ownerGuid = tripContext.TripList[trip].GetOwner();
                userContenxt.UserList[ownerGuid].AddScore(elem.Calificacion);
            }
        }

        public static CalificationContext CreateInsance()
        {
            if (_calificationInstance is null)
            {
                _calificationInstance = new CalificationContext();
            }
            return _calificationInstance;
        }

    }

    internal class CalificationTransfer
    {
        public string CalificationID { get; set; }
        public string PasageroID { get; set; }
        public string TripID { get; set; }
        public float Calificacion { get; set; }
        public string Comentario { get; set; }
    }
}
