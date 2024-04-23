﻿using System.Text.Json;
using Server.BL;

namespace DataAcces
{
    public class TripContext
    {
        private static TripContext _tripInstance = null;
        public Dictionary<Guid, Trip> TripList = new Dictionary<Guid, Trip>();

        private const string FileName = "Trips.json";
        private static Semaphore _tripSemaphore = new Semaphore(1, 1);
        private static Semaphore _mutexTrip = new Semaphore(1, 1);
        private static Semaphore _serviceQueueTrip = new Semaphore(1, 1);
        private static int _readersTrip = 0;

        private static string TripsFilePath
        {
            get
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
                parentDirectory = Directory.GetParent(parentDirectory.FullName);
                parentDirectory = Directory.GetParent(parentDirectory.FullName);
                return Path.Combine(parentDirectory.FullName, "Data", FileName);
            }
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

        public void LoadTripsFromTxt(UserContext context)
        {
            List<TripTransfer> source = new List<TripTransfer>();
            using (StreamReader r = new StreamReader(TripsFilePath))
            {
                string json = r.ReadToEnd();
                source = JsonSerializer.Deserialize<List<TripTransfer>>(json);
            }

            foreach (var elem in source)
            {
                Trip actual = new Trip()
                {
                    Origin = elem.Origen,
                    Destination = elem.Destino,
                    Departure = new DateTime(elem.Anio, elem.Mes, elem.Dia, elem.Hora, 0, 0),
                    AvailableSeats = elem.AsientosDisponibles,
                    TotalSeats = elem.AsientosTotales,
                    PricePerPassanger = elem.Precio,
                    Pet = elem.Mascota,
                    Photo = elem.photo
                };
                Guid actualGuid = new Guid(elem.TripID);
                actual.SetGuid(actualGuid);
                actual.SetOwner(new Guid(elem.OwnerID));
                List<Guid> passangers = new List<Guid>();
                foreach (var pass in elem.Pasageros)
                {
                    passangers.Add(new Guid(pass));
                    context.UserList[new Guid(pass)].Trips.Add(actualGuid);

                }
                actual.SetPassangers(passangers);

                _tripInstance.TripList.Add(actualGuid, actual);
            }
        }

        public static TripContext GetAccessWriteTrip()
        {
            _serviceQueueTrip.WaitOne();
            _tripSemaphore.WaitOne();
            _serviceQueueTrip.Release();

            //seccion critica
            return _tripInstance;
            //fin seccion critica
        }

        public static void ReturnWriteAccessTrip()
        {
            _tripSemaphore.Release();
        }

        public static TripContext CreateInsance()
        {
            _tripInstance = new TripContext();
            return _tripInstance;
        }
    }

    internal class TripTransfer
    {
        public string TripID { get; set; }
        public string OwnerID { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Dia { get; set; }
        public int Hora { get; set; }
        public int AsientosDisponibles { get; set; }
        public int AsientosTotales { get; set; }
        public float Precio { get; set; }
        public bool Mascota { get; set; }
        public string photo { get; set; }
        public string[] Pasageros { get; set; }
    }
}