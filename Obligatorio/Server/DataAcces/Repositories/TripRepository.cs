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
            TripContext context = TripContext.GetAccessWriteTrip();
            context.TripList.Add(trip.GetGuid(),trip);
            TripContext.ReturnWriteAccessTrip();
        }

        private void Add(Trip trip, TripContext context)
        {
            context.TripList.Add(trip.GetGuid(), trip);
        }

        public void Remove(Trip trip)
        {
            TripContext context = TripContext.GetAccessWriteTrip();
            Guid asociated = trip.GetGuid();
            context.TripList.Remove(asociated);
            TripContext.ReturnWriteAccessTrip();
        }

        public void Remove(Guid id)
        {
            TripContext context = TripContext.GetAccessWriteTrip();
            context.TripList.Remove(id);
            TripContext.ReturnWriteAccessTrip();
        }

        public Trip Get(Guid id)
        {
            Trip? asociated = null;
            TripContext context = TripContext.GetAccessReadTrip();
            context.TripList.TryGetValue(id, out asociated);
            TripContext.ReturnReadAccessTrip();
            if (asociated != null)
            {
                return asociated;
            }
            throw new TripManagerException($"Error 404, no se encuentra un Trip con el Guid {id}");
        }

        public void Update(Trip trip)
        {
            Guid id = trip.GetGuid();
            TripContext context = TripContext.GetAccessWriteTrip();
            if (context.TripList.ContainsKey(id))
            {
                context.TripList[id] = trip;
            }
            else
            {
                Add(trip, context);
            }
            TripContext.ReturnWriteAccessTrip();
        }

        /*
        private Guid GetGuid(Trip trip)
        {
            Guid asociated = Guid.Empty;
            TripContext context = TripContext.GetInstance();
            foreach (var elementTrip in context.TripList)
            {
                if (elementTrip.Equals(trip)) 
                {
                    asociated = elementTrip.Key;
                    break;
                }
            }
            TripContext.GetSemaphore().Release();
            return asociated;
        }
        */

        public List<Trip> GetAll()
        {
            TripContext context = TripContext.GetAccessReadTrip();
            List<Trip> all = new List<Trip>();
            foreach(var trip in context.TripList)
            {
                all.Add(trip.Value);
            }
            TripContext.ReturnReadAccessTrip();

            return all;
        }
    }
}
