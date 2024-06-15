using Microsoft.AspNetCore.Mvc;
using StatisticsServer.DTO;
using StatisticsServer.Repositories;
using System;

namespace StatisticsServer.Controllers
{
    [ApiController]
    [Route("api/trip-reports")]
    public class TripReportsController : ControllerBase
    {
        private readonly ITripReportRepository _tripReportRepository;

        public TripReportsController(ITripReportRepository tripReportRepository)
        {
            _tripReportRepository = tripReportRepository;
        }

        [HttpPost]
        public IActionResult CreateReport([FromBody] int numberOfTrips)
        {
            var report = new TripReport(numberOfTrips);
            _tripReportRepository.AddReport(report);
            return Ok(report.ReportId);
        }

        [HttpGet("{id}")]
        public IActionResult GetReport(Guid id)
        {
            var report = _tripReportRepository.GetReport(id);
            if (report == null)
            {
                return NotFound(new { message = "Report not found" });
            }

            if (!report.IsReady)
            {
                return Accepted(new { message = "Report is still in progress. Please check back later." });
            }

            return Ok(report);
        }

        [HttpGet("{reportId}/status")]
        public IActionResult GetReportStatus(Guid reportId)
        {
            var status = _tripReportRepository.GetReportStatus(reportId);
            if (status)
            {
                return Ok("Report is ready");
            }
            else
            {
                return Ok("Report is still in progress");
            }
        }
    }
}
