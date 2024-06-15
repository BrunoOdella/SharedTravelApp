using Microsoft.AspNetCore.Mvc;
using StatisticsServer.DTO;
using StatisticsServer.Repositories;

namespace StatisticsServer.Controllers
{
    [ApiController]
    [Route("api/trips")]
    public class TripsController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;

        public TripsController(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        [HttpGet]
        public IActionResult GetAllTrips()
        {
           return Ok(_tripRepository.GetAll());
        }


        [HttpGet("filter")]
        public IActionResult GetFilteredTrips([FromQuery] TripFilter filter)
        {
            var trips = _tripRepository.GetFilteredTrips(filter);
            return Ok(trips);
        }
    }
}

