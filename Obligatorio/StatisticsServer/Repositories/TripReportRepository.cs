using StatisticsServer.DTO;
using System;
using System.Collections.Concurrent;

namespace StatisticsServer.Repositories
{
    public interface ITripReportRepository
    {
        void AddReport(TripReport report);
        TripReport GetReport(Guid reportId);
        IEnumerable<TripReport> GetAllReports();
        void UpdateReport(TripReport report);
        bool GetReportStatus(Guid reportId);
    }
    public class TripReportRepository : ITripReportRepository
    {
        private readonly ConcurrentDictionary<Guid, TripReport> _reports = new();

        public void AddReport(TripReport report)
        {
            _reports[report.ReportId] = report;
        }

        public TripReport GetReport(Guid reportId)
        {
            _reports.TryGetValue(reportId, out var report);
            return report;
        }

        public IEnumerable<TripReport> GetAllReports()
        {
            return _reports.Values;
        }

        public void UpdateReport(TripReport report)
        {
            _reports[report.ReportId] = report;
        }

        public bool GetReportStatus(Guid reportId)
        {
            return _reports.TryGetValue(reportId, out var report) && report.IsReady;
        }
    }
}
