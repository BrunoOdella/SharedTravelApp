namespace GrpcServer.Server.BL.Repositories
{
    public interface IUserRepository
    {
        void Add(User user);
        void Update(User user);
        void Delete(User user);
        void Delete(Guid id);
        User Get(Guid id);
        List<User> GetAll();
        User GetUserByUsername(string username);

        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task DeleteAsync(Guid id);
        Task<User> GetAsync(Guid id);
        Task<List<User>> GetAllAsync();
        Task<User> GetUserByUsernameAsync(string username);
    }
}
