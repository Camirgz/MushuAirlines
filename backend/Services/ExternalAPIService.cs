using backend.Model;
using backend.Repositories;

namespace backend.Services;

public class ExternalApiService
{
    private readonly IExternalApiRepository _repo;

    public ExternalApiService(IExternalApiRepository repo)
    {
        _repo = repo;
    }

    public bool ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return false;
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(apiKey));
        var hash = Convert.ToHexString(bytes).ToLower();
        return _repo.ValidateApiKey(hash);
    }

    public string RegisterConsumer(string name)
    {
        var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(128);
        var apiKey = Convert.ToHexString(randomBytes).ToLower();

        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(apiKey));
        var hash = Convert.ToHexString(bytes).ToLower();

        var consumer = new APIConsumerModel
        {
            Name = name,
            ApiKeyHash = hash,
            IsActive = true
        };

        _repo.InsertConsumer(consumer);
        return apiKey;
    }

    public List<ExternalFlightDTO> GetFlights(string destination)
    {
        var rows = _repo.GetFlights(destination);

        return rows.Select(r => {
            var rowDict = (IDictionary<string, object>)r;

            string depTime = rowDict.ContainsKey("DepartureTime") && rowDict["DepartureTime"] != null
                ? Convert.ToString(rowDict["DepartureTime"])!.Trim() : "00:00";
            string arrTime = rowDict.ContainsKey("ArrivalTime") && rowDict["ArrivalTime"] != null
                ? Convert.ToString(rowDict["ArrivalTime"])!.Trim() : "00:00";

            return new ExternalFlightDTO
            {
                FlightGUID = rowDict.ContainsKey("FlightGUID") ? Convert.ToString(rowDict["FlightGUID"])! : "UNKNOWN",

                DepartureTime = depTime,
                ArrivalTime = arrTime,

                Duration = rowDict.ContainsKey("Duration") ? Convert.ToString(rowDict["Duration"])! : "00:00",
                OriginAirport = new AirportDTO
                {
                    Code = rowDict.ContainsKey("OriginAirport") ? Convert.ToString(rowDict["OriginAirport"])!.Trim() : "UNK",
                    AirportName = rowDict.ContainsKey("OriginName") && rowDict["OriginName"] != null ? Convert.ToString(rowDict["OriginName"])!.Trim() : "Default Airport",
                    City = rowDict.ContainsKey("OriginCity") && rowDict["OriginCity"] != null ? Convert.ToString(rowDict["OriginCity"])!.Trim() : "Unknown City"
                },
                ArrivalAirport = new AirportDTO
                {
                    Code = rowDict.ContainsKey("DestinationAirport") ? Convert.ToString(rowDict["DestinationAirport"])!.Trim() : "UNK",
                    AirportName = rowDict.ContainsKey("DestinationName") && rowDict["DestinationName"] != null ? Convert.ToString(rowDict["DestinationName"])!.Trim() : "Default Airport",
                    City = rowDict.ContainsKey("DestinationCity") && rowDict["DestinationCity"] != null ? Convert.ToString(rowDict["DestinationCity"])!.Trim() : "Unknown City"
                },
                PriceEconomyClass = rowDict.ContainsKey("PriceEconomy") && rowDict["PriceEconomy"] != null ? Convert.ToDecimal(rowDict["PriceEconomy"]) : 0m,
                PriceFirstClass = rowDict.ContainsKey("PriceFirstClass") && rowDict["PriceFirstClass"] != null ? Convert.ToDecimal(rowDict["PriceFirstClass"]) : 0m,
                HandbagPrice = rowDict.ContainsKey("HandBagPrice") && rowDict["HandBagPrice"] != null ? Convert.ToDecimal(rowDict["HandBagPrice"]) : 0m,
                BagPrice = rowDict.ContainsKey("BagPrice") && rowDict["BagPrice"] != null ? Convert.ToDecimal(rowDict["BagPrice"]) : 0m,
                Frequency = rowDict.ContainsKey("Frequency") ? Convert.ToString(rowDict["Frequency"])! : string.Empty
            };
        }).ToList();
    }

    public async Task<PurchaseResponseModel> OrderFlight(OrderRequest request)
    {
        // 1. Validar la tarjeta usando tu PaymentService existente
        _paymentService.ValidatePayment(request.Payment);

        ExternalAirlineFlightDto? matchedFlight = null;
        DateTime today = DateTime.Today;

        // --- NUEVA LÓGICA DINÁMICA ---
        // Obtenemos todas las rutas de Mushu de la base de datos.
        // Como tu repo ya tiene GetFlights(destination), podemos hacer un query general 
        // o simplemente extraer los destinos únicos usando una lista de tus aeropuertos destino conocidos.
        // Para este ejemplo, supongamos que mapeas los códigos destino válidos de tu sistema:
        var mushuDestinations = _repo.GetFlights("") // Si modificas tu consulta o si barres tus hubs conocidos (ej: "AMS", "MAD", "BOS", "JFK")
            .Select(r => {
                var dict = (IDictionary<string, object>)r;
                return dict.ContainsKey("DestinationAirport") ? Convert.ToString(dict["DestinationAirport"])!.Trim() : "";
            })
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct()
            .ToList();

        // Si por alguna razón la consulta viene vacía, usamos un fallback de tus destinos internacionales principales
        if (!mushuDestinations.Any())
        {
            mushuDestinations = new List<string> { "AMS", "MAD", "BOS", "MCO" };
        }

        // 2. Buscar el catálogo externo barriendo los próximos 7 días Y todos los destinos de Mushu
        bool flightFound = false;
        for (int i = 0; i < 7; i++)
        {
            string checkDate = today.AddDays(i).ToString("yyyy-MM-dd");

            foreach (var destinationAirport in mushuDestinations)
            {
                // Buscamos ofertas externas donde el destino de Mushu sea el punto de conexión (origen del externo)
                var externalFlights = await _aggregatorService.GetExternalFlightsAsync(
                    origin: null!,
                    originType: null!,
                    destination: destinationAirport, // Buscamos dinámicamente en cada aeropuerto destino de Mushu
                    destinationType: "airport",
                    date: checkDate
                );

                matchedFlight = externalFlights.FirstOrDefault(f => f.FlightGUID == request.FlightGUID);
                if (matchedFlight != null)
                {
                    flightFound = true;
                    break; // Lo encontramos, salimos del bucle de destinos
                }
            }

            if (flightFound) break; // Salimos del bucle de días
        }

        if (matchedFlight == null)
        {
            throw new Exception("El vuelo externo seleccionado no se encuentra operando o ya no está disponible en las rutas de conexión de Mushu Airlines.");
        }

        // 3. Mapear al DTO intermedio 'ExternalFlightSelection' que espera tu PurchaseService
        var externalFlightSelection = new ExternalFlightSelection
        {
            FlightGUID = matchedFlight.FlightGUID,
            AirlineName = matchedFlight.Airline,
            DepartureTime = matchedFlight.DepartureTime,
            ArrivalTime = matchedFlight.ArrivalTime,
            OriginAirport = matchedFlight.DepartureAirport?.Code ?? "UNK",
            DestinationAirport = matchedFlight.ArrivalAirport?.Code ?? "UNK",
            TouristPrice = matchedFlight.TouristPrice,
            FirstClassPrice = matchedFlight.FirstClassPrice,
            CarryOnPrice = matchedFlight.CarryOnPrice,
            CheckedPrice = matchedFlight.CheckedPrice
        };

        // 4. Construir las selecciones de asientos virtuales requeridos por el pipeline de Totales
        var seatSelections = request.Passengers.Select((p, index) => new SeatSelection
        {
            PassengerIndex = index,
            SeatClass = request.FirstClass ? SeatClassEnum.FirstClass : SeatClassEnum.Economy
        }).ToList();

        // 5. Armar el PurchaseRequestModel nativo de Mushu Airlines
        var purchaseRequest = new PurchaseRequestModel
        {
            Passengers = request.Passengers,
            Payment = request.Payment,
            SeatSelections = seatSelections,

            // Pasamos el vuelo externo en el Tramo 1 (Leg 1)
            ExternalFlight = externalFlightSelection,
            Flight = null,
            ExternalFlight2 = null,
            Flight2 = null
        };

        // 6. Ejecutar la transacción SQL centralizada de compras
        return await _purchaseService.CreatePurchaseAsync(purchaseRequest);
    }
}