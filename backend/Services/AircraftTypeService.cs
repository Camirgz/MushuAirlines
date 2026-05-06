using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class AircraftTypeService
    {
        private readonly AircraftTypeRepository _aircraftTypeRepository;

        public AircraftTypeService()
        {
            _aircraftTypeRepository = new AircraftTypeRepository();
        }

        public IEnumerable<AircraftTypeOptionModel> GetAll()
        {
            return _aircraftTypeRepository.GetAll();
        }
    }
}
