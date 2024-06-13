namespace StatisticsServer.DTO
{
    public class TripFilter
    {
        public string Destination { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
