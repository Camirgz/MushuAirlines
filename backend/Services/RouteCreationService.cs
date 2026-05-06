using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class RouteCreationService
    {
        private readonly RouteCreationRepository routeCreationRepository;

        public RouteCreationService()
        {
            routeCreationRepository = new RouteCreationRepository();
        }

        public string CreateRoute(RouteCreationModel route)
        {
            try
            {
                routeCreationRepository.InsertRoute(route);
                return "";
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

    }
}