namespace GrpcServer.Server.BL.Repositories
{
    public interface ITripRepository
    {
        void Add(Trip trip);
        void Remove(Trip trip);
        void Remove(Guid id);
        Trip Get(Guid id);
        void Update(Trip trip);
        List<Trip> GetAll();
        List<Trip> GetAll(Guid userGuid);

        List<Trip> GetAllTripsToOriginAndDestinationAvailableToJoin(string origin, string destination);
        List<Trip> GetAllTripsToOriginAndDestination(string origin, string destination);
        List<Trip> GetTripsFilteredByPetFriendly(bool petFriendly);
        List<Trip> GetTripsByOwner(Guid ownerId);
        bool isJoined(Guid tripId, Guid userId);
        bool isOwner(Guid tripId, Guid userId);
        List<Trip> FilterByDeparture(List<Trip> trips);

        Task AddAsync(Trip trip);
        Task RemoveAsync(Trip trip);
        Task RemoveAsync(Guid id);
        Task<Trip> GetAsync(Guid id);
        Task UpdateAsync(Trip trip);
        Task<List<Trip>> GetAllAsync();
        Task<List<Trip>> GetAllAsync(Guid userGuid);

        Task<List<Trip>> GetAllTripsToOriginAndDestinationAvailableToJoinAsync(string origin, string destination);
        Task<List<Trip>> GetAllTripsToOriginAndDestinationAsync(string origin, string destination);
        Task<List<Trip>> GetTripsFilteredByPetFriendlyAsync(bool petFriendly);
        Task<List<Trip>> GetTripsByOwnerAsync(Guid ownerId);
        Task<bool> IsJoinedAsync(Guid tripId, Guid userId);
        Task<bool> IsOwnerAsync(Guid tripId, Guid userId);
        Task<List<Trip>> FilterByDepartureAsync(List<Trip> trips);

    }
}
