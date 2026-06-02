using backend.Model;

namespace backend.Repositories;

public interface IFlightRepository
{
    IEnumerable<RouteDbModel> GetAll(
        string date = null,
        string origin = null,
        string originType = null,
        string destination = null,
        string destinationType = null);
}
