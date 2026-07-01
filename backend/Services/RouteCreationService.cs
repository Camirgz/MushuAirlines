using backend.Interfaces;
using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class RouteCreationService : IRouteCreationService
    {
        private readonly IRouteCreationRepository routeCreationRepository;

        public RouteCreationService()
        {
            routeCreationRepository = new RouteCreationRepository();
        }

        public RouteCreationService(IRouteCreationRepository repository)
        {
            routeCreationRepository = repository;
        }

        public string CreateRoute(RouteCreationModel route)
        {
            try
            {
                routeCreationRepository.InsertRoute(route);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public List<RouteCreationModel> GetRoutes()
        {
            var dbRoutes = routeCreationRepository.GetRoutes();

            return dbRoutes.Select(r => new RouteCreationModel
            {
                Code = r.Code,
                OriginAirport = r.OriginAirport,
                DestinationAirport = r.DestinationAirport,
                DepartureTime = r.DepartureTime,
                ArrivalTime = r.ArrivalTime,
                Duration = r.Duration,
                AircraftTypeId = r.AircraftTypeId,
                AircraftCode = r.AircraftCode,
                Frequency = r.Frequency.Split(',').ToList(),
                PriceFirstClass = r.PriceFirstClass,
                PriceEconomy = r.PriceEconomy,
                HandBagPrice = r.HandBagPrice,
                HandBagWeight = r.HandBagWeight,
                BagPrice = r.BagPrice,
                BagWeight = r.BagWeight,
                BagMultiplier = r.BagMultiplier,
                StartDate = r.StartDate,
                FinalizationDate = r.FinalizationDate,
                EconomyClassCapacity = r.EconomyClassCapacity,
                FirstClassCapacity = r.FirstClassCapacity,
                OriginCity = r.OriginCity,
                DestinationCity = r.DestinationCity
            }).ToList();
        }

        public int GetOrCreateScheduledFlight(string routeCode, DateTime date)
        {
            return routeCreationRepository.GetOrCreateScheduledFlight(routeCode, date);
        }

        public int? FindExistingScheduledFlight(string routeCode, DateTime date)
        {
            return routeCreationRepository.FindExistingScheduledFlight(routeCode, date);
        }

        public RouteCreationModel GetRouteByCode(string code)
        {
            RouteDbModel? r = routeCreationRepository.GetRouteByCode(code);

            if (r == null)
            {
                return null;
            }

            return new RouteCreationModel
            {
                Code = r.Code,
                OriginAirport = r.OriginAirport,
                DestinationAirport = r.DestinationAirport,
                DepartureTime = r.DepartureTime,
                ArrivalTime = r.ArrivalTime,
                Duration = r.Duration,
                AircraftTypeId = r.AircraftTypeId,
                AircraftCode = r.AircraftCode,
                Frequency = r.Frequency.Split(',').ToList(),
                PriceFirstClass = r.PriceFirstClass,
                PriceEconomy = r.PriceEconomy,
                HandBagPrice = r.HandBagPrice,
                HandBagWeight = r.HandBagWeight,
                BagPrice = r.BagPrice,
                BagWeight = r.BagWeight,
                BagMultiplier = r.BagMultiplier,
                StartDate = r.StartDate,
                FinalizationDate = r.FinalizationDate,
                EconomyClassCapacity = r.EconomyClassCapacity,
                FirstClassCapacity = r.FirstClassCapacity,
                OriginCity = r.OriginCity,
                DestinationCity = r.DestinationCity
            };
        }

        public void GetOrCreateExternalRoute(
            string flightGuid,
            string airlineName,
            string originAirport,
            string destinationAirport,
            string departureTime,
            string arrivalTime,
            string duration,
            decimal priceFirstClass,
            decimal priceEconomy,
            decimal handBagPrice,
            decimal bagPrice)
        {
            routeCreationRepository.GetOrCreateExternalRoute(
                flightGuid, airlineName, originAirport, destinationAirport,
                departureTime, arrivalTime, duration,
                priceFirstClass, priceEconomy, handBagPrice, bagPrice);
        }

        public string DeleteRoute(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return "La ruta seleccionada no es válida.";
            }

            try
            {
                bool wasDeleted = routeCreationRepository.DeleteRoute(code.Trim().ToUpper());

                if (!wasDeleted)
                {
                    return "No se encontró la ruta que desea eliminar.";
                }

                return string.Empty;
            }
            catch
            {
                return "No se pudo eliminar la ruta.";
            }
        }
    }
}
