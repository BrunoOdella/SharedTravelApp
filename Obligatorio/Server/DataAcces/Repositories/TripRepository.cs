using DataAcces;
using Server.BL;
using Server.BL.Repositories;
using Server.BL.BLException;

namespace Server.DataAcces.Repositories
{
    public class TripRepository : ITripRepository
    {
        public void Add(Trip trip)
        {
            Context context = Context.GetAccessWriteTrip();
            context.TripList.Add(trip.GetGuid(),trip);
            Context.ReturnWriteAccessTrip();
        }

        private void Add(Trip trip, Context context)
        {
            context.TripList.Add(trip.GetGuid(), trip);
        }

        public void Remove(Trip trip)
        {
            Context context = Context.GetAccessWriteTrip();
            Guid asociated = trip.GetGuid();
            context.TripList.Remove(asociated);
            Context.ReturnWriteAccessTrip();
        }

        public void Remove(Guid id)
        {
            Context context = Context.GetAccessWriteTrip();
            context.TripList.Remove(id);
            Context.ReturnWriteAccessTrip();
        }

        public Trip Get(Guid id)
        {
            Trip? asociated = null;
            Context context = Context.GetAccessReadTrip();
            context.TripList.TryGetValue(id, out asociated);
            Context.ReturnReadAccessTrip();
            if (asociated != null)
            {
                return asociated;
            }
            throw new TripManagerException($"Error 404, no se encuentra un Trip con el Guid {id}");
        }

        public void Update(Trip trip)
        {
            Guid id = trip.GetGuid();
            Context context = Context.GetAccessWriteTrip();
            if (context.TripList.ContainsKey(id))
            {
                context.TripList[id] = trip;
            }
            else
            {
                Add(trip, context);
            }
            Context.ReturnWriteAccessTrip();
        }

        /*
        private Guid GetGuid(Trip trip)
        {
            Guid asociated = Guid.Empty;
            Context context = Context.GetInstance();
            foreach (var elementTrip in context.TripList)
            {
                if (elementTrip.Equals(trip)) 
                {
                    asociated = elementTrip.Key;
                    break;
                }
            }
            Context.GetSemaphore().Release();
            return asociated;
        }
        */

        public List<Trip> GetAll()
        {
            Context context = Context.GetAccessReadTrip();
            List<Trip> all = new List<Trip>();
            foreach(var trip in context.TripList)
            {
                all.Add(trip.Value);
            }
            Context.ReturnReadAccessTrip();

            return all;
        }
    }
}
