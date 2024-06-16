using System.Text.Json.Serialization;

namespace StatisticsServer.DTO
{
    public class TripMessage
    {
        [JsonPropertyName("operation")]
        public string Operation { get; set; }
        public Guid TripId { get; set; }

        [JsonPropertyName("trip")]
        public Trip Trip { get; set; }
    }
}
