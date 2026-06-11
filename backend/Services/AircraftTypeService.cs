using backend.Interfaces;
using backend.Model;

namespace backend.Services
{
    public class AircraftTypeService : IAircraftTypeService
    {
        private readonly IAircraftTypeRepository _aircraftTypeRepository;

        public AircraftTypeService(IAircraftTypeRepository aircraftTypeRepository)
        {
            _aircraftTypeRepository = aircraftTypeRepository;
        }

        public IEnumerable<AircraftTypeOptionModel> GetAll()
        {
            return _aircraftTypeRepository.GetAll();
        }
    }
}
