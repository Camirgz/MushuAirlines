using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseConfirmationController : ControllerBase
    {
        private readonly
            PurchaseConfirmationService service;

        public PurchaseConfirmationController (PurchaseConfirmationService service)
        {
            this.service = service;
        }

        [HttpGet("{purchaseId}")]
        public ActionResult GetPurchase(int purchaseId)
        {
            try
            {
                var result = service.GetPurchaseData(purchaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("send/{purchaseId}")]
        public ActionResult SendConfirmation(int purchaseId)
        {
            try
            {
                var result = service.SendConfirmation(purchaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("resend/{purchaseId}")]
        public ActionResult ResendEmail(int purchaseId)
        {
            try
            {
                service.ResendEmail(purchaseId);
                return Ok("Correo reenviado correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}