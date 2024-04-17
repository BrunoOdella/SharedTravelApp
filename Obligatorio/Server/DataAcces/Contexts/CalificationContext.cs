using Server.BL;
using System.Text.Json;

namespace DataAcces
{
    public class CalificationContext
    {
        private static CalificationContext _calificationInstance = null;
        public Dictionary<Guid, Calification> CalificationList = new Dictionary<Guid, Calification>();
        private const string CalificationsFilePath = "D:\\Ort\\Prog de redes\\Obli\\Obligatorio\\Server\\Data\\Califications.json";//@"Data\Califications.txt";
        private static Semaphore _calificationSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexCalification = new Semaphore(1, 1);
        private static Semaphore _serviceQueueCalification = new Semaphore(1, 1);
        private static int _readersCalification = 0;

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

        public static void LoadCalificationsFromTxt()
        {
            List<CalificationTransfer> source = new List<CalificationTransfer>();
            using (StreamReader r = new StreamReader(CalificationsFilePath))
            {
                string json = r.ReadToEnd();
                source = JsonSerializer.Deserialize<List<CalificationTransfer>>(json);
            }

            foreach (var elem in source)
            {
                Guid guidActual = new Guid(elem._id);
                Calification actual = new Calification(new Guid(elem._passenger), new Guid(elem._trip), elem.calification, elem.Comment);
                actual.SetGuid(guidActual);
                _calificationInstance.CalificationList.Add(guidActual, actual);
            }
        }

        public static CalificationContext CreateInsance()
        {
            _calificationInstance = new CalificationContext();
            return _calificationInstance;
        }

    }

    internal class CalificationTransfer
    {
        public string _id { get; set; }
        public string _passenger { get; set; }
        public string _trip { get; set; }
        public float calification { get; set; }
        public string Comment { get; set; }
    }
}