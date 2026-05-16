using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PendingAccountController : ControllerBase
    {
        private readonly PendingAccountService pendingAccountService;

        public PendingAccountController()
        {
            pendingAccountService = new PendingAccountService();
        }

        [HttpPost]
        public ActionResult<string> CreateInvitation(PendingAccountModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }
            var result = pendingAccountService.CreateInvitation(model);

            if (result.Contains("Error:"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("completeRegister")]
        public ActionResult<string> CompleteRegister(CompleteRegisterModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var result = pendingAccountService.CompleteRegister(model);

            if (result == "Invalid or expired token" || result.Contains("Error:"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}