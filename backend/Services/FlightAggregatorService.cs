using backend.Model;
using backend.Repositories;

namespace backend.Services;

public class FlightAggregatorService
{
    private readonly IFlightRepository _flightRepository;

    public FlightAggregatorService(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }

    public IEnumerable<FlightDto> GetAllFlights()
    {
        return _flightRepository.GetAll().Select(ToDto);
    }

    private static FlightDto ToDto(RouteDbModel r) => new FlightDto
    {
        Code = r.Code,
        OriginAirport = r.OriginAirport,
        DestinationAirport = r.DestinationAirport,
        OriginCity = r.OriginCity,
        DestinationCity = r.DestinationCity,
        DepartureTime = r.DepartureTime,
        ArrivalTime = r.ArrivalTime,
        Duration = r.Duration,
        AircraftTypeId = r.AircraftTypeId,
        Frequency = r.Frequency != null
            ? r.Frequency.Split(',').Select(d => d.Trim()).Where(d => d.Length > 0).ToList()
            : new List<string>(),
        PriceFirstClass = r.PriceFirstClass,
        PriceEconomy = r.PriceEconomy,
        HandBagPrice = r.HandBagPrice,
        HandBagWeight = r.HandBagWeight,
        BagPrice = r.BagPrice,
        BagWeight = r.BagWeight,
        BagMultiplier = r.BagMultiplier,
        EconomyClassCapacity = r.EconomyClassCapacity,
        FirstClassCapacity = r.FirstClassCapacity,
    };
}
