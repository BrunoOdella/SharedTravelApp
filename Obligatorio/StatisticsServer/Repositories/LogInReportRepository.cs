using StatisticsServer.DTO;

namespace StatisticsServer.Repositories
{
    public interface ILogInReportRepository
    {
        void AddReport(LogInReport report);
        LogInReport GetReport(Guid reportId);
        List<LogInReport> GetAllReports();
        void UpdateReport(LogInReport report);
    }

    public class LoginReportRepository : ILogInReportRepository
    {
        private readonly List<LogInReport> _reports = new List<LogInReport>();

        public void AddReport(LogInReport report)
        {
            _reports.Add(report);
        }

        public LogInReport GetReport(Guid reportId)
        {
            return _reports.FirstOrDefault(r => r.ReportId == reportId);
        }

        public List<LogInReport> GetAllReports()
        {
            return _reports;
        }

        public void UpdateReport(LogInReport report)
        {
            var existingReport = GetReport(report.ReportId);
            if (existingReport != null)
            {
                _reports.Remove(existingReport);
                _reports.Add(report);
            }
        }
    }
}
