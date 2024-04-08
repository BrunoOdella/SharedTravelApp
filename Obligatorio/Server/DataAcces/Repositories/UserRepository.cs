using DataAcces;
using Server.BL;
using Server.BL.BLException;
using Server.BL.Repositories;

namespace Server.DataAcces.Repositories;

public class UserRepository : IUserRepository
{
    public void Add(User user)
    {
        UserContext context = UserContext.GetAccessWriteUser();
        context.UserList.Add(user.GetGuid(), user);
        UserContext.ReturnWriteAccessUser();
    }

    private void Add(User user, UserContext context)
    {
        context.UserList.Add(user.GetGuid(), user);
    }

    public void Delete(User user)
    {
        UserContext context = UserContext.GetAccessWriteUser();
        Guid asociated = user.GetGuid();
        context.UserList.Remove(asociated);
        UserContext.ReturnWriteAccessUser();
    }

    public void Delete(Guid id)
    {
        UserContext context = UserContext.GetAccessWriteUser();
        context.UserList.Remove(id);
        UserContext.ReturnWriteAccessUser();
    }

    public User Get(Guid id)
    {
        User? asociated = null;
        UserContext context = UserContext.GetAccessReadUser();
        context.UserList.TryGetValue(id, out asociated);
        UserContext.ReturnReadAccessUser();
        if (asociated != null)
        {
            return asociated;
        }
        throw new UserManagerException($"Error 404, no se encuentra un User con el Guid {id}");
    }

    public List<User> GetAll()
    {
        UserContext context = UserContext.GetAccessReadUser();
        List<User> all = new List<User>();
        foreach (var user in context.UserList)
        {
            all.Add(user.Value);
        }
        UserContext.ReturnReadAccessUser();

        return all;
    }

    public void Update(User user)
    {
        Guid id = user.GetGuid();
        UserContext context = UserContext.GetAccessWriteUser();
        if (context.UserList.ContainsKey(id))
        {
            context.UserList[id] = user;
        }
        else
        {
            Add(user, context);
        }
        UserContext.ReturnWriteAccessUser();
    }
}