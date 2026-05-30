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
