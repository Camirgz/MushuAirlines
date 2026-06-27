using backend.DTOs;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/reports/flights")]
    [ApiController]
    [Authorize]
    public class FlightReportsController : ControllerBase
    {
        private readonly IFlightDetailReportService _flightDetailService;
        private readonly IMonthlyIncomeReportService _monthlyIncomeService;

        public FlightReportsController(
            IFlightDetailReportService flightDetailService,
            IMonthlyIncomeReportService monthlyIncomeService)
        {
            _flightDetailService  = flightDetailService;
            _monthlyIncomeService = monthlyIncomeService;
        }

        [HttpGet("detail")]
        public ActionResult GetFlightDetail(
            [FromQuery] string? origen,
            [FromQuery] string? destino,
            [FromQuery] string? clase,
            [FromQuery] string? fechaDesde,
            [FromQuery] string? fechaHasta)
        {
            try
            {
                var filter = BuildFlightDetailFilter(origen, destino, clase, fechaDesde, fechaHasta);
                var result = _flightDetailService.GetFlightDetail(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("detail/excel")]
        public ActionResult ExportFlightDetailExcel(
            [FromQuery] string? origen,
            [FromQuery] string? destino,
            [FromQuery] string? clase,
            [FromQuery] string? fechaDesde,
            [FromQuery] string? fechaHasta)
        {
            try
            {
                var filter = BuildFlightDetailFilter(origen, destino, clase, fechaDesde, fechaHasta);
                var bytes  = _flightDetailService.ExportToExcel(filter);

                return File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ReporteVueloDetallado.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("monthly")]
        public ActionResult GetMonthlyIncome(
            [FromQuery] int? anio,
            [FromQuery] string? origen,
            [FromQuery] string? destino,
            [FromQuery] string? aerolinea)
        {
            try
            {
                var filter = new MonthlyIncomeReportFilterDto
                {
                    Anio      = anio,
                    Origen    = NullIfEmpty(origen),
                    Destino   = NullIfEmpty(destino),
                    Aerolinea = NullIfEmpty(aerolinea)
                };

                var result = _monthlyIncomeService.GetMonthlyIncome(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("detail/pdf")]
        public ActionResult ExportFlightDetailPdf(
            [FromQuery] string? origen,
            [FromQuery] string? destino,
            [FromQuery] string? clase,
            [FromQuery] string? fechaDesde,
            [FromQuery] string? fechaHasta)
        {
            try
            {
                var filter = BuildFlightDetailFilter(origen, destino, clase, fechaDesde, fechaHasta);
                var bytes  = _flightDetailService.ExportToPdf(filter);

                return File(bytes, "application/pdf", "ReporteVueloDetallado.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("monthly/excel")]
        public ActionResult ExportMonthlyIncomeExcel(
            [FromQuery] int? anio,
            [FromQuery] string? origen,
            [FromQuery] string? destino,
            [FromQuery] string? aerolinea)
        {
            try
            {
                var filter = new MonthlyIncomeReportFilterDto
                {
                    Anio      = anio,
                    Origen    = NullIfEmpty(origen),
                    Destino   = NullIfEmpty(destino),
                    Aerolinea = NullIfEmpty(aerolinea)
                };

                var bytes = _monthlyIncomeService.ExportToExcel(filter);

                return File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ReporteIngresosPorMes.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("monthly/pdf")]
        public ActionResult ExportMonthlyIncomePdf(
            [FromQuery] int? anio,
            [FromQuery] string? origen,
            [FromQuery] string? destino,
            [FromQuery] string? aerolinea)
        {
            try
            {
                var filter = new MonthlyIncomeReportFilterDto
                {
                    Anio      = anio,
                    Origen    = NullIfEmpty(origen),
                    Destino   = NullIfEmpty(destino),
                    Aerolinea = NullIfEmpty(aerolinea)
                };

                var bytes = _monthlyIncomeService.ExportToPdf(filter);

                return File(bytes, "application/pdf", "ReporteIngresosPorMes.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private FlightDetailReportFilterDto BuildFlightDetailFilter(
            string? origen, string? destino, string? clase,
            string? fechaDesde, string? fechaHasta)
        {
            DateOnly? desde = null;
            DateOnly? hasta = null;

            if (!string.IsNullOrWhiteSpace(fechaDesde) &&
                DateOnly.TryParse(fechaDesde, out var d))
                desde = d;

            if (!string.IsNullOrWhiteSpace(fechaHasta) &&
                DateOnly.TryParse(fechaHasta, out var h))
                hasta = h;

            return new FlightDetailReportFilterDto
            {
                Origen     = NullIfEmpty(origen),
                Destino    = NullIfEmpty(destino),
                Clase      = NullIfEmpty(clase),
                FechaDesde = desde,
                FechaHasta = hasta
            };
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
