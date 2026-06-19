using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService loginService;

        public LoginController(ILoginService loginService)
        {
            this.loginService = loginService;
        }

        [HttpPost]
        public ActionResult<string> Login(LoginModel login)
        {
            if (login == null)
            {
                return BadRequest();
            }

            var result = loginService.Login(login);

            if (result == "Usuario o contraseña incorrectos")
            {
                return BadRequest(result);
            }
           
            // else we have here the token to return to the frontend
            return Ok(result); 
        }
    }
}