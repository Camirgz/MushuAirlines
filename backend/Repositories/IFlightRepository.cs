using backend.Model;

namespace backend.Repositories;

public interface IFlightRepository
{
    IEnumerable<RouteDbModel> GetAll();
}
