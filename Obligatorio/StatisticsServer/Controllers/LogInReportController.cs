using Microsoft.AspNetCore.Mvc;
using StatisticsServer.DTO;
using StatisticsServer.Repositories;

namespace StatisticsServer.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class LoginReportsController : ControllerBase
    {
        private readonly ILogInReportRepository _loginReportRepository;

        public LoginReportsController(ILogInReportRepository loginReportRepository)
        {
            _loginReportRepository = loginReportRepository;
        }

        [HttpPost]
        public IActionResult CreateReport([FromBody] int numberOfLogins)
        {
            var report = new LogInReport(numberOfLogins);
            _loginReportRepository.AddReport(report);
            return Ok(report.ReportId);
        }

        [HttpGet("{id}")]
        public IActionResult GetReport(Guid id)
        {
            var report = _loginReportRepository.GetReport(id);
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
            var status = _loginReportRepository.GetReportStatus(reportId);
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
