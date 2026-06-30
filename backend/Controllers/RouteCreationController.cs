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

        return BadRequest(result);
    }

    [HttpGet]
    public ActionResult GetRoutes()
    {
        var routes = routeCreationService.GetRoutes();
        return Ok(routes);
    }

    [HttpGet("{code}")]
    public ActionResult GetRouteByCode(string code)
    {
        var route = routeCreationService.GetRouteByCode(code);

        if (route == null)
        {
            return NotFound("No se encontró la ruta solicitada.");
        }

        return Ok(route);
    }

    [HttpDelete("{code}")]
    public ActionResult DeleteRoute(string code)
    {
        string result = routeCreationService.DeleteRoute(code);

        if (string.IsNullOrEmpty(result))
        {
            return Ok("Ruta eliminado correctamente.");
        }

        if (result == "No se encontró la ruta que desea eliminar.")
        {
            return NotFound(result);
        }

        return BadRequest(result);
    }
}
