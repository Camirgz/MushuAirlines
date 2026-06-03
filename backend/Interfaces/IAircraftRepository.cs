using backend.Model;

namespace backend.Interfaces
{
    public interface IAircraftRepository
    {
        IEnumerable<AircraftResponseModel> GetAll();
        AircraftResponseModel? GetById(int id);
        bool AircraftExists(string model, string type);
        void Create(CreateAircraftRequestModel aircraft);
        void Update(int id, UpdateAircraftRequestModel aircraft);
    }
}
