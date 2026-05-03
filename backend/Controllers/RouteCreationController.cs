using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RouteCreationController : ControllerBase
{
    private readonly RouteCreationService routeCreationService;

    public RouteCreationController()
    {
        routeCreationService = new RouteCreationService();
    }


    [HttpPost]
    public ActionResult CreateRoute([FromBody] RouteCreationModel route)
    {
         if (route == null)
         {
            return BadRequest();
         }

        var result = routeCreationService.CreateRoute(route);

        if (string.IsNullOrEmpty(result))
        {
            return Ok("Ruta creada correctamente");
        }
        else
        {
            return BadRequest(result);
        }
    }

    [HttpGet]
    public ActionResult GetRoutes()
    {
        var routes = routeCreationService.GetRoutes();
        return Ok(routes);
    }
}