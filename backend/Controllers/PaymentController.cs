using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService service;

        public PaymentController(IPaymentService paymentService)
        {
            this.service = paymentService;
        }

        [HttpPost("approve")]
        public ActionResult ApprovePayment([FromBody] PaymentModel model)
        {
            try
            {
                int purchaseId =
                    service.ApprovePayment(model);

                return Ok(new
                {
                    PurchaseId = purchaseId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}