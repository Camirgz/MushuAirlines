using backend.Model;

namespace backend.Interfaces
{
    public interface IAircraftService
    {
        IEnumerable<AircraftResponseModel> GetAll();
        AircraftResponseModel? GetById(int id);
        string Create(CreateAircraftRequestModel aircraft);
        string Update(int id, UpdateAircraftRequestModel aircraft);
        string Delete(int id);
    }
}
