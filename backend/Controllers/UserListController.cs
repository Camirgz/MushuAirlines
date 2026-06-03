using backend.Interfaces;
using backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrator")]
public class UserListController : ControllerBase
{
    private readonly IUserListService _userListService;

    public UserListController(IUserListService userListService)
    {
        _userListService = userListService;
    }

    [HttpGet]
    public ActionResult GetUsers([FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        const int pageSize = 10;

        var result = _userListService.GetUsers(page, pageSize, search);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public ActionResult UpdateUser(int id, [FromBody] UserManagementUpdateModel user)
    {
        string result = _userListService.UpdateUser(id, user);

        if (!string.IsNullOrWhiteSpace(result))
        {
            return BadRequest(result);
        }

        return Ok("Usuario actualizado correctamente.");
    }
}

