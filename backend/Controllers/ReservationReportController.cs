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
        private readonly IPdfItineraryService pdfService;
        public ReservationReportController(
            IReservationReportService service,
            IPdfItineraryService pdfService)
        {
            this.service = service;
            this.pdfService = pdfService;
        }

        [HttpGet]
        public ActionResult GetReservation()
        {
            try
            {
               var reservationCode = User.FindFirst("ReservationCode")?.Value;
                var result =service.GetReservation(reservationCode!);
                
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
        [HttpGet("pdf")]
        public ActionResult PrintItinerary()
        {
            try
            {
                var reservationCode =
                    User.FindFirst("ReservationCode")?.Value;

                var reservation =
                    service.GetReservation(reservationCode!);

                var pdf =
                    pdfService.GeneratePdf(reservation);

                return File(
                    pdf,
                    "application/pdf",
                    $"Itinerary-{reservation.ReservationCode}.pdf");
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