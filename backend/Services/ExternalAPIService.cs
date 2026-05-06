using backend.Model;
using backend.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services;

public class ExternalApiService
{
	private readonly ExternalApiRepository _repo;

	public ExternalApiService()
	{
		_repo = new ExternalApiRepository();
	}

	public bool ValidateApiKey(string apiKey)
	{
		var hash = HashApiKey(apiKey);
		return _repo.ValidateApiKey(hash);
	}

	public string RegisterConsumer(string name)
	{

		var randomBytes = RandomNumberGenerator.GetBytes(128);
		var apiKey = Convert.ToHexString(randomBytes).ToLower();

		var consumer = new APIConsumerModel
		{
			Name = name,
			ApiKeyHash = HashApiKey(apiKey),
			IsActive = true
		};

		_repo.InsertConsumer(consumer);
		return apiKey;
	}

	public (bool valid, string errorCode, string errorDescription) ValidateQueryParams(
		string origin, string destination, string earliestDeparture,
		string latestDeparture, int quantityOfPassengers)
	{
		if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
			return (false, "MISSING_AIRPORTS", "Origin and destination airports are required");

		if (!DateTime.TryParse(earliestDeparture, out _) || !DateTime.TryParse(latestDeparture, out _))
			return (false, "INVALID_DATE_FORMAT", "Dates must be in ISO format YYYY-MM-DDThh:mm");

		if (quantityOfPassengers < 1)
			return (false, "INVALID_PASSENGERS", "quantityOfPassengers must be >= 1");

		if (DateTime.Parse(earliestDeparture) > DateTime.Parse(latestDeparture))
			return (false, "INVALID_DATE_RANGE", "earliestDeparture must be before latestDeparture");

		return (true, "", "");
	}

	public List<ExternalFlightDTO> GetFlights(string origin, string destination,
		string earliestDeparture, string latestDeparture, int quantityOfPassengers)
	{
		var earliest = DateTime.Parse(earliestDeparture);
		var latest = DateTime.Parse(latestDeparture);

		var rows = _repo.GetFlights(origin, destination, earliest, latest);

		return rows.Select(r => new ExternalFlightDTO
		{
			FlightGUID = r.FlightGUID,
			DepartureTime = Convert.ToDateTime(r.DepartureTime).ToString("yyyy-MM-ddTHH:mm"),
			ArrivalTime = Convert.ToDateTime(r.ArrivalTime).ToString("yyyy-MM-ddTHH:mm"),
			Duration = r.Duration,
			OriginAirport = new AirportDTO
			{
				Code = r.OriginAirport,
				AirportName = r.OriginName,
				City = r.OriginCity
			},
			ArrivalAirport = new AirportDTO
			{
				Code = r.DestinationAirport,
				AirportName = r.DestinationName,
				City = r.DestinationCity
			},
			PriceEconomyClass = r.PriceEconomy,
			PriceFirstClass = r.PriceFirstClass,
			HandbagPrice = r.HandBagPrice,
			BagPrice = r.BagPrice
		}).ToList();
	}

	private static string HashApiKey(string apiKey)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
		return Convert.ToHexString(bytes).ToLower();
	}
}