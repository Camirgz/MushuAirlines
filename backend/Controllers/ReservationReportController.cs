using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationReportController : ControllerBase
    {
        private readonly IReservationReportService service;

        public ReservationReportController(
            IReservationReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public ActionResult GetReservation()
        {
            try
            {
               var reservationCode = User.FindFirst("ReservationCode")?.Value;
                var result =
                    service.GetReservation(reservationCode!);

                return Ok(result);
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