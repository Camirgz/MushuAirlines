using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class AircraftService
    {
        private readonly AircraftRepository _aircraftRepository;

        public AircraftService()
        {
            _aircraftRepository = new AircraftRepository();
        }

        public IEnumerable<AircraftResponseModel> GetAll()
        {
            return _aircraftRepository.GetAll();
        }

        public string Create(CreateAircraftRequestModel aircraft)
        {
            if (_aircraftRepository.AircraftExists(aircraft.Model, aircraft.Type))
                return "Ya existe una aeronave con el modelo \"" + aircraft.Model + "\" y tipo \"" + aircraft.Type + "\".";

            int TotalSeats = aircraft.EconomyRows * aircraft.EconomySeatsPerRow
                           + (aircraft.FirstClassRows ?? 0) * (aircraft.FirstClassSeatsPerRow ?? 1);

            if (TotalSeats >= 1000)
                return "La capacidad total de asientos (" + TotalSeats + ") no puede ser igual o mayor a 1000.";

            var result = string.Empty;
            try
            {
                _aircraftRepository.Create(aircraft);
            }
            catch (Exception ex)
            {
                result = "ERROR: " + ex.Message;
            }
            return result;
        }
    }
}
