using backend.Interfaces;
using backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationCancellationController : ControllerBase
    {
        private readonly IReservationCancellationService service;

        public ReservationCancellationController(
            IReservationCancellationService service)
        {
            this.service = service;
        }

        [HttpPost("sendEmailCancellation")]
        public IActionResult SendCancellationEmail()
        {
            try
            {
                var reservationCode =
                    User.FindFirst("ReservationCode")?.Value;

                service.SendCancellationEmail(reservationCode!);

                return Ok(new
                {
                    message = "Correo enviado correctamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("cancel/{token}")]
        public IActionResult CancelReservation(string token)
        {
            try
            {
                service.CancelReservation(token);

                return Ok(new
                {
                    message = "Reserva cancelada correctamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}