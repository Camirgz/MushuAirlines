using backend.Interfaces;
using backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AircraftController : ControllerBase
    {
        private readonly IAircraftService _aircraftService;

        public AircraftController(IAircraftService aircraftService)
        {
            _aircraftService = aircraftService;
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

        [HttpGet("{id}")]
        public ActionResult<AircraftResponseModel> GetById(int id)
        {
            try
            {
                var aircraft = _aircraftService.GetById(id);

                if (aircraft == null)
                {
                    return NotFound("No se encontró la aeronave solicitada.");
                }

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
            {
                return BadRequest("Los datos de la aeronave son requeridos.");
            }

            var error = _aircraftService.Create(aircraft);

            if (string.IsNullOrEmpty(error))
            {
                return Ok();
            }

            return BadRequest(error);
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, UpdateAircraftRequestModel aircraft)
        {
            if (aircraft == null)
            {
                return BadRequest("Los datos de la aeronave son requeridos.");
            }

            var error = _aircraftService.Update(id, aircraft);

            if (string.IsNullOrEmpty(error))
            {
                return Ok();
            }

            if (error == "No se encontró la aeronave solicitada.")
            {
                return NotFound(error);
            }

            return BadRequest(error);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var error = _aircraftService.Delete(id);

            if (string.IsNullOrEmpty(error))
            {
                return Ok("Aeronave eliminada correctamente.");
            }

            if (error == "No se encontró la aeronave que desea eliminar.")
            {
                return NotFound(error);
            }

            return BadRequest(error);
        }
    }
}
