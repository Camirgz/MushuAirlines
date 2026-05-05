using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AircraftController : ControllerBase
    {
        private readonly AircraftService _aircraftService;

        public AircraftController()
        {
            _aircraftService = new AircraftService();
        }

        [HttpGet]
        public ActionResult<IEnumerable<AircraftResponseModel>> GetAll()
        {
            try
            {
                var aircraft = _aircraftService.GetAll();
                return Ok(aircraft);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateAircraftRequestModel aircraft)
        {
            if (aircraft == null)
                return BadRequest();

            var error = _aircraftService.Create(aircraft);

            if (string.IsNullOrEmpty(error))
                return Ok();
            else
                return BadRequest(error);
        }
    }
}
