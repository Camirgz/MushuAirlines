using backend.Model;
using backend.Repositories;

namespace backend.Services;

public class FlightAggregatorService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IExternalAirlinesService _externalAirlinesService;

    public FlightAggregatorService(
        IFlightRepository flightRepository,
        IExternalAirlinesService externalAirlinesService)
    {
        _flightRepository = flightRepository;
        _externalAirlinesService = externalAirlinesService;
    }

    public IEnumerable<FlightDto> GetAllFlights(
        string date = null,
        string origin = null,
        string originType = null,
        string destination = null,
        string destinationType = null)
    {
        return _flightRepository.GetAll(date, origin, originType, destination, destinationType).Select(ToDto);
    }

    public async Task<IEnumerable<ExternalAirlineFlightDto>> GetExternalFlightsAsync(
        string origin,
        string originType,
        string destination,
        string destinationType,
        string date)
    {
        if (string.IsNullOrWhiteSpace(destination) || destinationType != "airport")
            return Enumerable.Empty<ExternalAirlineFlightDto>();

        // Siempre buscar vuelos al destino final (A→C directo o leg2 de escala X→C)
        var airportCodes = new HashSet<string> { destination };

        // Si el origen es IATA, buscar también los destinos intermedios de Mushu desde ese origen
        // para poder armar escalas: Mushu A→B + Externo B→C
        if (originType == "airport" && !string.IsNullOrWhiteSpace(origin))
        {
            var intermediates = _flightRepository.GetIntermediateDestinations(origin);
            foreach (var code in intermediates)
                if (code != destination)
                    airportCodes.Add(code);
        }

        var tasks = airportCodes.Select(code => _externalAirlinesService.GetExternalFlightsAsync(code, date));
        var results = await Task.WhenAll(tasks);

        return results.SelectMany(r => r)
            .GroupBy(f => f.FlightGUID + f.DepartureTime)
            .Select(g => g.First());
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
        FinalizationDate = r.FinalizationDate,
    };
}
