namespace GrpcServer.Server.BL
{
    public class Calification
    {
        private Guid _userId;
        private Guid _tripId;
        private Guid _calificationId;


        public float Score { get; set; }
        public string Comment { get; set; }

        public Calification(Guid userId, Guid tripId, float score, string comment)
        {
            this._userId = userId;
            this._tripId = tripId;
            this.Score = score;
            this.Comment = comment;
            this._calificationId = new Guid();
        }

        public Guid GetGuid()
        {
            return _calificationId;
        }

        public void SetGuid(Guid id)
        {
            _calificationId = id;
        }

        public Guid GetTrip()
        {
            return _tripId;
        }

    }
}
