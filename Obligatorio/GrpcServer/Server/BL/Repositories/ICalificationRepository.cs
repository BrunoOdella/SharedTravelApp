namespace GrpcServer.Server.BL.Repositories
{
    public interface ICalificationRepository
    {
        Task AddAsync(Calification calification);
        Task UpdateAsync(Calification calification);
        Task DeleteAsync(Calification calification);
        Task DeleteAsync(Guid id);
        Task<Calification> GetAsync(Guid id);
        Task<List<Calification>> GetAllAsync();
        Task<List<Calification>> GetCalificationsByTripIdAsync(Guid tripId);
    }
}
