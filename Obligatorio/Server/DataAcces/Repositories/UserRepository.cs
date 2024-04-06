using DataAcces;
using Server.BL;
using Server.BL.BLException;
using Server.BL.Repositories;

namespace Server.DataAcces.Repositories;

public class UserRepository : IUserRepository
{
    public void Add(User user)
    {
        Context context = Context.GetAccessWriteUser();
        context.UserList.Add(user.GetGuid(), user);
        Context.ReturnWriteAccessUser();
    }

    private void Add(User user, Context context)
    {
        context.UserList.Add(user.GetGuid(), user);
    }

    public void Delete(User user)
    {
        Context context = Context.GetAccessWriteUser();
        Guid asociated = user.GetGuid();
        context.UserList.Remove(asociated);
        Context.ReturnWriteAccessUser();
    }

    public void Delete(Guid id)
    {
        Context context = Context.GetAccessWriteUser();
        context.UserList.Remove(id);
        Context.ReturnWriteAccessUser();
    }

    public User Get(Guid id)
    {
        User? asociated = null;
        Context context = Context.GetAccessReadUser();
        context.UserList.TryGetValue(id, out asociated);
        Context.ReturnReadAccessUser();
        if (asociated != null)
        {
            return asociated;
        }
        throw new UserManagerException($"Error 404, no se encuentra un User con el Guid {id}");
    }

    public List<User> GetAll()
    {
        Context context = Context.GetAccessReadUser();
        List<User> all = new List<User>();
        foreach (var user in context.UserList)
        {
            all.Add(user.Value);
        }
        Context.ReturnReadAccessUser();

        return all;
    }

    public void Update(User user)
    {
        Guid id = user.GetGuid();
        Context context = Context.GetAccessWriteUser();
        if (context.UserList.ContainsKey(id))
        {
            context.UserList[id] = user;
        }
        else
        {
            Add(user, context);
        }
        Context.ReturnWriteAccessUser();
    }
}