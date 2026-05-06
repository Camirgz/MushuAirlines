using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserListController : ControllerBase
{
    private readonly UserListService _userListService;

    public UserListController()
    {
        _userListService = new UserListService();
    }

    [HttpGet]
    public ActionResult GetUsers([FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        const int pageSize = 10;

        var result = _userListService.GetUsers(page, pageSize, search);
        return Ok(result);
    }
}
