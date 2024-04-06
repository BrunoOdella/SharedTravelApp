namespace Server.BL
{
    public class User
    {
        private Guid _id;
        private string _password;

        public string Name { get; set; }
        public List<Guid> Trip;

        public User()
        {
            this._id = Guid.Empty; //generar un numero acorde a algo
        }

        public Guid GetGuid()
        {
            return _id;
        }
    }
}
