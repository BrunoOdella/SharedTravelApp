namespace Server.BL.Repositories;

public interface IUserRepository
{
    void Add(User user);
    void Update(User user);
    void Delete(User user);
    void Delete(Guid id);
    User Get(Guid id);
    List<User> GetAll();
    User GetUserByUsername(string username);
}