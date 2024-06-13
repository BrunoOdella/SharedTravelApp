namespace StatisticsServer.DTO
{
    public class TripFilter
    {
        public string Destination { get; set; }
        public float? MaxPrice { get; set; }
        public DateTime? Date { get; set; }
    }
}
