using backend.Exceptions;
using backend.Interfaces;
using backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet("check-availability")]
    public async Task<ActionResult> CheckAvailability(
        [FromQuery] string routeCode,
        [FromQuery] string flightDate,
        [FromQuery] int firstClassCount,
        [FromQuery] int economyCount)
    {
        if (!DateOnly.TryParse(flightDate, out DateOnly date))
            return BadRequest(new { message = "Formato de fecha inválido." });

        bool available = await _purchaseService.IsFlightAvailableAsync(routeCode, date, firstClassCount, economyCount);
        return Ok(new { available });
    }

    [HttpPost("check-passenger-duplicates")]
    public async Task<ActionResult> CheckPassengerDuplicates(
        [FromBody] PassengerDuplicateCheckRequest request)
    {
        var duplicates = await _purchaseService.CheckPassengerDuplicatesAsync(
            request.RouteCode, request.FlightDate, request.Passengers);
        return Ok(new { hasDuplicates = duplicates.Count > 0, duplicates });
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseResponseModel>> CreatePurchase(
        [FromBody] PurchaseRequestModel request)
    {
        try
        {
            var response = await _purchaseService.CreatePurchaseAsync(request);
            return StatusCode(201, response);
        }
        catch (PassengerDataException ex)
        {
            return BadRequest(new { message = ex.Message, field = ex.Field });
        }
        catch (InvalidFlightDateException ex)
        {
            return UnprocessableEntity(new
            {
                message     = ex.Message,
                routeCode   = ex.RouteCode,
                requestedDate = ex.RequestedDate
            });
        }
        catch (SeatUnavailableException ex)
        {
            return Conflict(new { message = ex.Message, seatNumber = ex.SeatNumber });
        }
    }
}
