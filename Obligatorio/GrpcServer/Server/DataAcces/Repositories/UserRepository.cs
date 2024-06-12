using GrpcServer.Server.BL.BLException;
using GrpcServer.Server.DataAcces.Contexts;
using GrpcServer.Server.BL.Repositories;
using GrpcServer.Server.BL;

namespace GrpcServer.Server.DataAcces.Repositories
{
    public class UserRepository : IUserRepository
    {
        public void Add(User user)
        {
            Task<UserContext> contextTask = UserContext.GetAccessWriteUser();
            UserContext context = contextTask.Result;
            context.UserList.Add(user.GetGuid(), user);
            UserContext.ReturnWriteAccessUser();
        }

        private void Add(User user, UserContext context)
        {
            context.UserList.Add(user.GetGuid(), user);
        }

        public void Delete(User user)
        {
            Task<UserContext> contextTask = UserContext.GetAccessWriteUser();
            UserContext context = contextTask.Result;
            Guid asociated = user.GetGuid();
            context.UserList.Remove(asociated);
            UserContext.ReturnWriteAccessUser();
        }

        public void Delete(Guid id)
        {
            Task<UserContext> contextTask = UserContext.GetAccessWriteUser();
            UserContext context = contextTask.Result;
            context.UserList.Remove(id);
            UserContext.ReturnWriteAccessUser();
        }

        public User Get(Guid id)
        {
            User? asociated = null;
            Task<UserContext> contextTask = UserContext.GetAccessReadUser();
            UserContext context = contextTask.Result;
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
            Task<UserContext> contextTask = UserContext.GetAccessReadUser();
            UserContext context = contextTask.Result;
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
            Task<UserContext> contextTask = UserContext.GetAccessWriteUser();
            UserContext context = contextTask.Result;
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

        public User GetUserByUsername(string username)
        {
            Task<UserContext> contextTask = UserContext.GetAccessReadUser();
            UserContext context = contextTask.Result;
            User user = context.UserList.Values.FirstOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
            UserContext.ReturnReadAccessUser();

            if (user != null)
            {
                return user;
            }
            else
            {
                return null;
            }
        }
    }
}
