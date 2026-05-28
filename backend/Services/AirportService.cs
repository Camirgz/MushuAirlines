using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class AirportService
    {
        private readonly AirportRepository airportRepository;

        public AirportService()
        {
            airportRepository = new AirportRepository();
        }

        public List<string> GetCountries()
        {
            return airportRepository.GetCountries();
        }

        public List<string> GetCitiesByCountry(string country)
        {
            return airportRepository.GetCitiesByCountry(country);
        }

        public List<AirportModel> GetAirports()
        {
            return airportRepository.GetAirports();
        }

        public string CreateAirport(AirportModel airport)
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

                if (!airportRepository.CityBelongsToCountry(airport.Country, airport.City))
                {
                    return "La ciudad seleccionada no pertenece al país seleccionado.";
                }

                if (airportRepository.AirportCodeExists(airport.Code))
                {
                    return "Ya existe un aeropuerto con ese código.";
                }

                if (airportRepository.AirportNameExists(
                    airport.AirportName,
                    airport.Country,
                    airport.City
                ))
                {
                    return "Ya existe un aeropuerto con ese nombre en la misma ciudad y país.";
                }

                airportRepository.InsertAirport(airport);

                return "";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }
    }
}
