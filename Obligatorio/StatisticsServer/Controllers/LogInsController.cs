using Microsoft.AspNetCore.Mvc;
using StatisticsServer.Repositories;

namespace StatisticsServer.Controllers
{
    [ApiController]
    [Route("api/logins")]
    public class LogInsController : ControllerBase
    {
        private readonly ILoginEventRepository _loginEventRepository;

        public LogInsController(ILoginEventRepository loginEventRepository)
        {
            _loginEventRepository = loginEventRepository;
        }

        [HttpGet]
        public IActionResult GetAllLoginEvents()
        {
            return Ok(_loginEventRepository.GetAll());
        }

        [HttpGet("logInCounts")]
        public IActionResult GetUniqueUserCount()
        {
            int count = _loginEventRepository.GetUserLogInCount();
            return Ok(new { UniqueUserCount = count });
        }
    }
}
