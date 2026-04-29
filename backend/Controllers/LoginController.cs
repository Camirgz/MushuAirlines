using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginService loginService;

        public LoginController()
        {
            loginService = new LoginService();
        }

        [HttpPost]
        public ActionResult<bool> Login(LoginModel login)
        {
            if (login == null)
            {
                return BadRequest();
            }

            var result = loginService.Login(login);

            if (string.IsNullOrEmpty(result))
            {
                // login correcto
                return Ok(true); 
            }
            else
            {
                // error
                return BadRequest(result); 
            }
        }
    }
}