namespace StatisticsServer.DTO
{
    public class Trip
    {
        public Guid Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime Departure { get; set; }
        public int AvailableSeats { get; set; }
        public float PricePerPassanger { get; set; }

        public Trip()
        {
            this.Id = Guid.NewGuid();
        }

        internal void SetGuid(Guid tripId)
        {
            this.Id = tripId;
        }
    }
}
