using System.Security.Cryptography;

namespace Server.BL
{
    public class User
    {
        private Guid _id;
        private string _password;

        public string Name { get; set; }
        public List<Guid> Trips;

        public User()
        {
            this._id = Guid.NewGuid();
        }

        public void AddTrip(Guid id)
        {
            this.Trips.Add(id);
        }


        public Guid GetGuid()
        {
            return _id;
        }

        public void SetGuid(Guid id)
        {
            _id = id;
        }

        public void SetPassword(string password)
        {
            _password = password;
        }
    }
}
