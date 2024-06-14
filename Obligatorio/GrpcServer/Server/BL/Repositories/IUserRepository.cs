namespace GrpcServer.Server.BL.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task DeleteAsync(Guid id);
        Task<User> GetAsync(Guid id);
        Task<List<User>> GetAllAsync();
        Task<User> GetUserByUsernameAsync(string username);
    }
}
