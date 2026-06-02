using backend.Interfaces;
using backend.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public ActionResult GetMyProfile()
    {
        string? username = GetCurrentUsername();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized("No se pudo identificar el usuario conectado.");
        }

        ProfileModel? profile = _profileService.GetProfileByUsername(username);

        if (profile == null)
        {
            return NotFound("No existe un perfil asociado al usuario conectado.");
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    public ActionResult UpdateMyProfile([FromBody] ProfileUpdateModel profile)
    {
        string? username = GetCurrentUsername();
        string? role = GetCurrentRole();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized("No se pudo identificar el usuario conectado.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized("No se pudo identificar el rol del usuario conectado.");
        }

        string result = _profileService.UpdateProfile(username, role, profile);

        if (!string.IsNullOrWhiteSpace(result))
        {
            return BadRequest(result);
        }

        return Ok("Perfil actualizado correctamente.");
    }

    private string? GetCurrentUsername()
    {
        return
            User.FindFirstValue(ClaimTypes.Name) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("unique_name") ??
            User.FindFirstValue("username") ??
            User.FindFirstValue("sub");
    }

    private string? GetCurrentRole()
    {
        string? role =
            User.FindFirstValue(ClaimTypes.Role) ??
            User.FindFirstValue("role") ??
            User.FindFirstValue("Role");

        if (role == "Administrador")
        {
            return "Administrator";
        }

        if (role == "Operador")
        {
            return "Operator";
        }

        return role;
    }
}
