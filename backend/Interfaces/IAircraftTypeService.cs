using backend.Model;

namespace backend.Interfaces
{
    public interface IAircraftTypeService
    {
        IEnumerable<AircraftTypeOptionModel> GetAll();
    }
}
