using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class AirportCreationService
    {
        private readonly AirportCreationRepository airportCreationRepository;

        public AirportCreationService()
        {
            airportCreationRepository = new AirportCreationRepository();
        }

        public List<string> GetCountries()
        {
            return airportCreationRepository.GetCountries();
        }

        public List<string> GetCitiesByCountry(string country)
        {
            return airportCreationRepository.GetCitiesByCountry(country);
        }

        public List<AirportCreationModel> GetAirports()
        {
            return airportCreationRepository.GetAirports();
        }

        public string CreateAirport(AirportCreationModel airport)
        {
            try
            {
                if (airport == null)
                {
                    return "Debe ingresar los datos del aeropuerto.";
                }

                if (string.IsNullOrWhiteSpace(airport.Country))
                {
                    return "Debe seleccionar un país.";
                }

                if (string.IsNullOrWhiteSpace(airport.City))
                {
                    return "Debe seleccionar una ciudad.";
                }

                if (string.IsNullOrWhiteSpace(airport.AirportName))
                {
                    return "Debe ingresar el nombre del aeropuerto.";
                }

                if (string.IsNullOrWhiteSpace(airport.Code))
                {
                    return "Debe ingresar el código del aeropuerto.";
                }

                airport.Code = airport.Code.Trim().ToUpper();
                airport.AirportName = airport.AirportName.Trim();
                airport.Country = airport.Country.Trim();
                airport.City = airport.City.Trim();

                if (airport.Code.Length != 3)
                {
                    return "El código del aeropuerto debe tener exactamente 3 caracteres.";
                }

                if (!airportCreationRepository.CityBelongsToCountry(airport.Country, airport.City))
                {
                    return "La ciudad seleccionada no pertenece al país seleccionado.";
                }

                if (airportCreationRepository.AirportCodeExists(airport.Code))
                {
                    return "Ya existe un aeropuerto con ese código.";
                }

                if (airportCreationRepository.AirportNameExists(
                    airport.AirportName,
                    airport.Country,
                    airport.City
                ))
                {
                    return "Ya existe un aeropuerto con ese nombre en la misma ciudad y país.";
                }

                airportCreationRepository.InsertAirport(airport);

                return "";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }
}
