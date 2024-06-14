namespace StatisticsServer.DTO
{
    public class LogInReport
    {
        public Guid ReportId { get; set; }
        public int RequiredLogins { get; set; }
        public List<LoginEvent> Logins { get; set; }
        public bool IsReady { get; set; }
        public DateTime? ReadyAt { get; set; }

        public LogInReport(int requiredLogins)
        {
            ReportId = Guid.NewGuid();
            RequiredLogins = requiredLogins;
            Logins = new List<LoginEvent>();
            IsReady = false;
            ReadyAt = null;
        }
    }
}
