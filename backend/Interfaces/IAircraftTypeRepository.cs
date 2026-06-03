using backend.Model;

namespace backend.Interfaces
{
    public interface IAircraftTypeRepository
    {
        IEnumerable<AircraftTypeOptionModel> GetAll();
    }
}
