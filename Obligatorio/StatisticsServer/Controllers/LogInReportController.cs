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

        [HttpGet("{reportId}")]
        public IActionResult GetReport(Guid reportId)
        {
            var report = _loginReportRepository.GetReport(reportId);
            if (report == null)
            {
                return NotFound();
            }

            return Ok(report);
        }
    }
}
