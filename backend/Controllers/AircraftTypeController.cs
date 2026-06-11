using backend.Interfaces;
using backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/aircraft-type")]
    [ApiController]
    public class AircraftTypeController : ControllerBase
    {
        private readonly IAircraftTypeService _aircraftTypeService;

        public AircraftTypeController(IAircraftTypeService aircraftTypeService)
        {
            _aircraftTypeService = aircraftTypeService;
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
