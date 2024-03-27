using DataAcces;
using Server.BL;
using Server.BL.Repositories;
using Server.BL.BLException;

namespace Server.DataAcces.Repositories
{
    public class TripRepository : ITripRepository
    {
        public void AddTrip(Trip trip)
        {
            Context context = Context.GetInstance();
            context.TripList.Add(new Guid(),trip);
            Context.GetSemaphore().Release();
        }

        private void AddTrip(Trip trip, Context context)
        {
            context.TripList.Add(new Guid(), trip);
        }

        public void RemoveTrip(Trip trip)
        {
            Context context = Context.GetInstance();
            Guid asociado = GetGuid(trip);
            context.TripList.Remove(asociado);
            Context.GetSemaphore().Release();
        }

        public void RemoveTrip(Guid id)
        {
            Context context = Context.GetInstance();
            Guid asociado = id;
            context.TripList.Remove(asociado);
            Context.GetSemaphore().Release();
        }

        public Trip GetTrip(Guid id)
        {
            Trip? asociado = null;
            Context contex = Context.GetInstance();
            contex.TripList.TryGetValue(id, out asociado);
            Context.GetSemaphore().Release();
            if (asociado != null)
            {
                return asociado;
            }
            throw new TripManagerException($"Error 404, no se encuentra un Trip con el Guid {id}");
        }

        public void UpdateTrip(Trip trip, Guid? id)
        {
            if (id == null)
            {
                id = GetGuid(trip);
            }
            Context context = Context.GetInstance();
            if (context.TripList.ContainsKey((Guid) id))
            {
                context.TripList[(Guid) id] = trip;
            }
            else
            {
                AddTrip(trip, context);
            }
            Context.GetSemaphore().Release();
        }

        private Guid GetGuid(Trip trip)
        {
            Guid asociado = Guid.Empty;
            Context context = Context.GetInstance();
            foreach (var elementTrip in context.TripList)
            {
                if (elementTrip.Equals(trip)) 
                {
                    asociado = elementTrip.Key;
                    break;
                }
            }
            Context.GetSemaphore().Release();
            return asociado;
        }
    }
}
