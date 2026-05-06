using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/aircraft-type")]
    [ApiController]
    public class AircraftTypeController : ControllerBase
    {
        private readonly AircraftTypeService _aircraftTypeService;

        public AircraftTypeController()
        {
            _aircraftTypeService = new AircraftTypeService();
        }

        [HttpGet]
        public ActionResult<IEnumerable<AircraftTypeOptionModel>> GetAll()
        {
            try
            {
                var types = _aircraftTypeService.GetAll();
                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
