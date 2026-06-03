using backend.Interfaces;
using backend.Model;

namespace backend.Services
{
    public class AircraftService : IAircraftService
    {
        private readonly IAircraftRepository _aircraftRepository;
        
        private const int MaxSeatCapacity = 1000;

        public AircraftService(IAircraftRepository aircraftRepository)
        {
            _aircraftRepository = aircraftRepository;
        }

        public IEnumerable<AircraftResponseModel> GetAll()
        {
            return _aircraftRepository.GetAll();
        }

        public AircraftResponseModel? GetById(int id)
        {
            return _aircraftRepository.GetById(id);
        }

        public string Create(CreateAircraftRequestModel aircraft)
        {
            if (_aircraftRepository.AircraftExists(aircraft.Model, aircraft.Type))
            {
                return "Ya existe una aeronave con el modelo \"" + aircraft.Model + "\" y tipo \"" + aircraft.Type + "\".";
            }

            int firstClassRows = aircraft.FirstClassRows ?? 0;
            int firstClassSeatsPerRow = aircraft.FirstClassSeatsPerRow ?? 1;

            int totalSeats =
                aircraft.EconomyRows * aircraft.EconomySeatsPerRow
                + firstClassRows * firstClassSeatsPerRow;

            if (totalSeats >= MaxSeatCapacity)
            {
                return "La capacidad total de asientos (" + totalSeats + ") no puede ser igual o mayor a 1000.";
            }

            if (aircraft.WeightKg <= 0)
            {
                return "El peso soportado debe ser mayor a 0.";
            }

            if (aircraft.EconomyRows < 0 || aircraft.EconomySeatsPerRow < 0)
            {
                return "Los valores de clase turista no pueden ser negativos.";
            }

            if (firstClassRows < 0 || firstClassSeatsPerRow < 0)
            {
                return "Los valores de primera clase no pueden ser negativos.";
            }

            try
            {
                _aircraftRepository.Create(aircraft);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public string Update(int id, UpdateAircraftRequestModel aircraft)
        {
            var currentAircraft = _aircraftRepository.GetById(id);

            if (currentAircraft == null)
            {
                return "No se encontró la aeronave solicitada.";
            }

            if (aircraft.WeightKg < currentAircraft.WeightKg)
            {
                return "El peso soportado no puede disminuir.";
            }

            if (aircraft.EconomyRows < currentAircraft.EconomyRows)
            {
                return "La cantidad de filas de clase turista no puede disminuir.";
            }

            if (aircraft.EconomySeatsPerRow < currentAircraft.EconomySeatsPerRow)
            {
                return "Los asientos por fila de clase turista no pueden disminuir.";
            }

            if (aircraft.FirstClassRows < currentAircraft.FirstClassRows)
            {
                return "La cantidad de filas de primera clase no puede disminuir.";
            }

            if (aircraft.FirstClassSeatsPerRow < currentAircraft.FirstClassSeatsPerRow)
            {
                return "Los asientos por fila de primera clase no pueden disminuir.";
            }

            bool hasIncreasedValue =
                aircraft.WeightKg > currentAircraft.WeightKg ||
                aircraft.EconomyRows > currentAircraft.EconomyRows ||
                aircraft.EconomySeatsPerRow > currentAircraft.EconomySeatsPerRow ||
                aircraft.FirstClassRows > currentAircraft.FirstClassRows ||
                aircraft.FirstClassSeatsPerRow > currentAircraft.FirstClassSeatsPerRow;

            if (!hasIncreasedValue)
            {
                return "Debe aumentar al menos un valor para actualizar la aeronave.";
            }

            int totalSeats =
                aircraft.EconomyRows * aircraft.EconomySeatsPerRow
                + aircraft.FirstClassRows * aircraft.FirstClassSeatsPerRow;

            if (totalSeats >= 1000)
            {
                return $"La capacidad total de asientos ({totalSeats}) no puede ser igual o mayor a 1000.";
            }

            try
            {
                _aircraftRepository.Update(id, aircraft);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }
}
