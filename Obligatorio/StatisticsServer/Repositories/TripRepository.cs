using StatisticsServer.DTO;

namespace StatisticsServer.Repositories
{
    public interface ITripRepository
    {
        void Add(Trip trip);
        IEnumerable<Trip> GetAll();
        IEnumerable<Trip> GetFilteredTrips(TripFilter filter);
    }

    public class TripRepository : ITripRepository
    {
        private readonly List<Trip> _trips = new List<Trip>();

        public void Add(Trip trip)
        {
            _trips.Add(trip);
        }

        public IEnumerable<Trip> GetAll()
        {
            return _trips;
        }

        public IEnumerable<Trip> GetFilteredTrips(TripFilter filter)
        {
            return _trips.Where(t =>
                (string.IsNullOrEmpty(filter.Destination) || t.Destination == filter.Destination) &&
                /*(!filter.MaxPrice.HasValue || t.PricePerPassanger <= filter.MaxPrice.Value) &&*/
                (!filter.StartDate.HasValue || t.Departure >= filter.StartDate.Value) &&
                (!filter.EndDate.HasValue || t.Departure <= filter.EndDate.Value));
        }
    }
}
