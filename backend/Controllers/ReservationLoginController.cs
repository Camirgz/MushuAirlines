using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationLoginController : ControllerBase
    {
        private readonly IReservationLoginService service;

        public ReservationLoginController(IReservationLoginService service)
        {
            this.service = service;
        }

        [HttpPost]
        public IActionResult Login(ReservationLoginModel model)
        {
            if (model == null) { 
                return BadRequest();
            }

            var result = service.ReservationLogin(model);

            if (result=="Reserva no encontrada") {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}