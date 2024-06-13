namespace StatisticsServer.DTO
{
    public class Trip
    {
        public Guid Id { get; set; }
        public Guid Owner { get; set; }
        public List<Guid> Passengers { get; set; }

        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime Departure { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
        public float PricePerPassanger { get; set; }
        public bool Pet { get; set; }
        public string Photo { get; set; }

        public Trip()
        {
            this.Id = Guid.NewGuid();
            this.Passengers = new List<Guid>();
        }
    }
}
