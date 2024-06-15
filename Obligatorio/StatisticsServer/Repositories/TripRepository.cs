using StatisticsServer.DTO;
using System.Collections.Concurrent;

namespace StatisticsServer.Repositories
{
    public interface ITripRepository
    {
        void Add(Trip trip);
        void Update(Trip trip);
        void Delete(Guid tripId);
        IEnumerable<Trip> GetAll();
        IEnumerable<Trip> GetFilteredTrips(TripFilter filter);
    }

    public class TripRepository : ITripRepository
    {
        private readonly ConcurrentDictionary<Guid, Trip> _trips = new ConcurrentDictionary<Guid, Trip>();

        public void Add(Trip trip)
        {
            _trips[trip.Id] = trip;
        }

        public void Update(Trip trip)
        {
            _trips[trip.Id] = trip;
        }

        public void Delete(Guid tripId)
        {
            _trips.TryRemove(tripId, out _);
        }

        public IEnumerable<Trip> GetAll()
        {
            return _trips.Values;
        }

        public IEnumerable<Trip> GetFilteredTrips(TripFilter filter)
        {
            return _trips.Values.Where(t =>
                (string.IsNullOrEmpty(filter.Destination) || t.Destination == filter.Destination) &&
                (!filter.MaxPrice.HasValue || t.PricePerPassanger <= filter.MaxPrice.Value) &&
                (!filter.Date.HasValue || t.Departure == filter.Date.Value));
        }
    }
}
