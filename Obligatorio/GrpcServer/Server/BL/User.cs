namespace GrpcServer.Server.BL
{
    public class User
    {
        public Guid _id;
        public string _password;
        private float _score = 0;
        private int _scoreCount = 0;

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

        public void AddScore(float score)
        {
            _score += score;
            _scoreCount++;
        }

        public float GetScore()
        {
            return _score == 0 ? 0 : _score / _scoreCount;
        }
    }
}
