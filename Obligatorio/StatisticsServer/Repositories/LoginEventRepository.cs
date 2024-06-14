using StatisticsServer.DTO;

namespace StatisticsServer.Repositories
{
    public interface ILoginEventRepository
    {
        void Add(LoginEvent loginEvent);
        List<LoginEvent> GetAll();
    }

    public class LoginEventRepository : ILoginEventRepository
    {
        private readonly List<LoginEvent> _loginEvents = new();

        public void Add(LoginEvent loginEvent)
        {
            _loginEvents.Add(loginEvent);
        }

        public List<LoginEvent> GetAll()
        {
            return _loginEvents;
        }
    }
}
