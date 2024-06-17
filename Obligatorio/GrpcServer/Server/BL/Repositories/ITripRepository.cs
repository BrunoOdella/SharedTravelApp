namespace GrpcServer.Server.BL.Repositories
{
    public interface ITripRepository
    {
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
