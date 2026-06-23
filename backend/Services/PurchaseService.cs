using backend.Exceptions;
using backend.Interfaces;
using backend.Model;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPassengerRepository  _passengerRepo;
    private readonly IPurchaseRepository   _purchaseRepo;
    private readonly ICodeGenerator        _codeGenerator;
    private readonly IPricingCalculator    _pricingCalculator;
    private readonly IRouteCreationService _routeCreationService;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseService(
        IPassengerRepository  passengerRepo,
        IPurchaseRepository   purchaseRepo,
        ICodeGenerator        codeGenerator,
        IPricingCalculator    pricingCalculator,
        IRouteCreationService routeCreationService,
        ILogger<PurchaseService> logger)
    {
        _passengerRepo        = passengerRepo;
        _purchaseRepo         = purchaseRepo;
        _codeGenerator        = codeGenerator;
        _pricingCalculator    = pricingCalculator;
        _routeCreationService = routeCreationService;
        _logger               = logger;
    }

    public async Task<PurchaseResponseModel> CreatePurchaseAsync(PurchaseRequestModel request)
    {
        ValidatePassengers(request.Passengers);

        RouteCreationModel route1;
        try
        {
            route1 = _routeCreationService.GetRouteByCode(request.Flight.RouteCode);
        }
        catch
        {
            throw new InvalidFlightDateException(
                request.Flight.FlightDate,
                request.Flight.RouteCode,
                "la ruta no existe o no está disponible");
        }

        int scheduledFlightId1 = _routeCreationService.GetOrCreateScheduledFlight(
            request.Flight.RouteCode,
            request.Flight.FlightDate.ToDateTime(TimeOnly.MinValue));

        bool isStopover          = request.Flight2 != null;
        RouteCreationModel? route2      = null;
        int                scheduledFlightId2 = 0;

        if (isStopover)
        {
            try
            {
                route2 = _routeCreationService.GetRouteByCode(request.Flight2!.RouteCode);
            }
            catch
            {
                throw new InvalidFlightDateException(
                    request.Flight2!.FlightDate,
                    request.Flight2.RouteCode,
                    "la ruta del segundo tramo no existe o no está disponible");
            }

            scheduledFlightId2 = _routeCreationService.GetOrCreateScheduledFlight(
                request.Flight2.RouteCode,
                request.Flight2.FlightDate.ToDateTime(TimeOnly.MinValue));
        }

        int seatCount       = request.SeatSelections.Count;
        int firstClassCount = request.SeatSelections.Count(s => s.SeatClass == SeatClass.FirstClass);
        int economyCount    = request.SeatSelections.Count(s => s.SeatClass == SeatClass.Economy);

        bool flight1Available = await IsFlightAvailableAsync(
            request.Flight.RouteCode, request.Flight.FlightDate, firstClassCount, economyCount);
        if (!flight1Available) throw new SeatUnavailableException(scheduledFlightId1);

        if (isStopover)
        {
            bool flight2Available = await IsFlightAvailableAsync(
                request.Flight2!.RouteCode, request.Flight2.FlightDate, firstClassCount, economyCount);
            if (!flight2Available) throw new SeatUnavailableException(scheduledFlightId2);
        }

        var seatNumberResults = await Task.WhenAll(
            _purchaseRepo.GetNextAvailableSeatNumbersAsync(scheduledFlightId1, seatCount),
            isStopover
                ? _purchaseRepo.GetNextAvailableSeatNumbersAsync(scheduledFlightId2, seatCount)
                : Task.FromResult<List<int>>([]));

        var assignedSeatNumbers1  = seatNumberResults[0];
        List<int>? assignedSeatNumbers2 = isStopover ? seatNumberResults[1] : null;

        decimal economyPrice    = route1.PriceEconomy    + (isStopover ? route2!.PriceEconomy    : 0);
        decimal firstClassPrice = route1.PriceFirstClass + (isStopover ? route2!.PriceFirstClass : 0);
        decimal handBagPrice    = route1.HandBagPrice    + (isStopover ? route2!.HandBagPrice    : 0);
        decimal bagPrice        = route1.BagPrice        + (isStopover ? route2!.BagPrice        : 0);
        decimal bagMultiplier   = route1.BagMultiplier;

        var totals = _pricingCalculator.Calculate(
            request.SeatSelections,
            request.Passengers,
            economyPrice,
            firstClassPrice,
            handBagPrice,
            bagPrice,
            bagMultiplier);

        var uniqueCodes = await Task.WhenAll(
            GenerateUniqueReservationCodeAsync(),
            GenerateUniqueInvoiceNumberAsync());

        var reservationCode = uniqueCodes[0];
        var invoiceNumber   = uniqueCodes[1];

        var passengerIds = await Task.WhenAll(
            request.Passengers.Select(p => _passengerRepo.CreatePassengerAsync(p)));

        var firstPassengerId = passengerIds[0];
        var purchaseDate     = DateTime.UtcNow;

        var tickets1 = request.SeatSelections
            .Select((seat, i) => new TicketInsertData
            {
                ScheduledFlightId = scheduledFlightId1,
                PassengerId       = passengerIds[seat.PassengerIndex],
                SeatNumber        = assignedSeatNumbers1[i],
                SeatClass         = seat.SeatClass.ToString()
            })
            .ToList();

        var ticketBaggage1 = request.SeatSelections
            .Select(seat =>
            {
                var bag = totals.PassengerBaggageDetails[seat.PassengerIndex];
                return new TicketBaggageInsertData
                {
                    ScheduledFlightId = scheduledFlightId1,
                    PassengerId       = passengerIds[seat.PassengerIndex],
                    HandBagCount      = bag.HandBagCount,
                    CheckedBagCount   = bag.CheckedBagCount,
                    Subtotal          = bag.Subtotal
                };
            })
            .ToList();

        List<TicketInsertData>?        tickets2       = null;
        List<TicketBaggageInsertData>? ticketBaggage2 = null;

        if (isStopover)
        {
            tickets2 = request.SeatSelections
                .Select((seat, i) => new TicketInsertData
                {
                    ScheduledFlightId = scheduledFlightId2,
                    PassengerId       = passengerIds[seat.PassengerIndex],
                    SeatNumber        = assignedSeatNumbers2![i],
                    SeatClass         = seat.SeatClass.ToString()
                })
                .ToList();

            ticketBaggage2 = request.SeatSelections
                .Select(seat =>
                {
                    var bag = totals.PassengerBaggageDetails[seat.PassengerIndex];
                    return new TicketBaggageInsertData
                    {
                        ScheduledFlightId = scheduledFlightId2,
                        PassengerId       = passengerIds[seat.PassengerIndex],
                        HandBagCount      = bag.HandBagCount,
                        CheckedBagCount   = bag.CheckedBagCount,
                        Subtotal          = bag.Subtotal
                    };
                })
                .ToList();
        }

        var purchaseId = await _purchaseRepo.ExecutePurchaseTransactionAsync(new PurchaseTransactionData
        {
            Record = new PurchaseRecord
            {
                PassengerId     = firstPassengerId,
                ReservationCode = reservationCode,
                InvoiceNumber   = invoiceNumber,
                PaymentMethod   = request.Payment.Method,
                Email           = request.Payment.ContactEmail,
                TotalPaid       = totals.TotalPaid,
                TotalSeats      = totals.TotalSeats,
                PurchaseDate    = purchaseDate
            },
            Details        = totals.DetailByClass,
            BaggageDetails = totals.BaggageDetails,
            Tickets1       = tickets1,
            TicketBaggage1 = ticketBaggage1,
            ScheduledId1   = scheduledFlightId1,
            Tickets2       = tickets2,
            TicketBaggage2 = ticketBaggage2,
            ScheduledId2   = isStopover ? scheduledFlightId2 : null
        });

        var tickets = GenerateTicketSummaries(
            request.SeatSelections,
            request.Passengers,
            assignedSeatNumbers1,
            assignedSeatNumbers2,
            isStopover,
            scheduledFlightId1,
            isStopover ? scheduledFlightId2 : null);

        return new PurchaseResponseModel
        {
            PurchaseId      = purchaseId,
            ReservationCode = reservationCode,
            InvoiceNumber   = invoiceNumber,
            TotalPaid       = totals.TotalPaid,
            TotalSeats      = totals.TotalSeats,
            PurchaseDate    = purchaseDate,
            DetailByClass   = totals.DetailByClass,
            Tickets         = tickets
        };
    }


    private static List<TicketSummary> GenerateTicketSummaries(
        List<SeatSelection> seatSelections,
        List<PassengerInfo> passengers,
        List<int> seatNumbers1,
        List<int>? seatNumbers2,
        bool isStopover,
        int scheduledFlightId1,
        int? scheduledFlightId2)
    {
    var tickets = new List<TicketSummary>();

    // Vuelo 1
    tickets.AddRange(seatSelections.Select((seat, i) =>
    {
        var passenger = passengers[seat.PassengerIndex];

        return new TicketSummary
        {
            PassengerFullName = $"{passenger.FirstName} {passenger.LastName}",
            SeatNumber = seatNumbers1[i].ToString(),
            SeatClass = seat.SeatClass,
            FlightNumber = scheduledFlightId1.ToString()
        };
    }));

    // Vuelo 2
    if (isStopover && seatNumbers2 != null && scheduledFlightId2.HasValue)
    {
        tickets.AddRange(seatSelections.Select((seat, i) =>
        {
            var passenger = passengers[seat.PassengerIndex];

            return new TicketSummary
            {
                PassengerFullName = $"{passenger.FirstName} {passenger.LastName}",
                SeatNumber = seatNumbers2[i].ToString(),
                SeatClass = seat.SeatClass,
                FlightNumber = scheduledFlightId2.Value.ToString()
            };
        }));
    }

    return tickets;
}

    public async Task<bool> IsFlightAvailableAsync(
        string routeCode, DateOnly flightDate, int firstClassCount, int economyCount)
    {
        RouteCreationModel route;
        try { route = _routeCreationService.GetRouteByCode(routeCode); }
        catch (Exception ex)
        {
            _logger.LogWarning("IsFlightAvailableAsync: route '{RouteCode}' not found — {Msg}", routeCode, ex.Message);
            return true;
        }

        int fcCapacity  = route.FirstClassCapacity;
        int ecoCapacity = route.EconomyClassCapacity;

        if (fcCapacity == 0 && ecoCapacity == 0)
        {
            var (fc, eco) = await _purchaseRepo.GetAircraftCapacityByClassAsync(route.AircraftTypeId);
            fcCapacity  = fc;
            ecoCapacity = eco;
            _logger.LogInformation(
                "IsFlightAvailableAsync: route capacities were 0, fell back to aircraft — FC_cap={FC} Eco_cap={Eco}",
                fcCapacity, ecoCapacity);
        }

        _logger.LogInformation(
            "IsFlightAvailableAsync: route={RouteCode} FC_cap={FC} Eco_cap={Eco} requested FC={RqFC} Eco={RqEco}",
            routeCode, fcCapacity, ecoCapacity, firstClassCount, economyCount);

        var flightDateTime     = flightDate.ToDateTime(TimeOnly.MinValue);
        int? scheduledFlightId = _routeCreationService.FindExistingScheduledFlight(routeCode, flightDateTime);

        _logger.LogInformation("IsFlightAvailableAsync: scheduledFlightId={SFId}", scheduledFlightId);

        if (firstClassCount > 0 && fcCapacity > 0)
        {
            int bookedFC = scheduledFlightId.HasValue
                ? await _purchaseRepo.GetBookedSeatsByClassAsync(scheduledFlightId.Value, "FirstClass")
                : 0;
            _logger.LogInformation("IsFlightAvailableAsync: bookedFC={B} requested={R} cap={C}", bookedFC, firstClassCount, fcCapacity);
            if (firstClassCount + bookedFC > fcCapacity) return false;
        }

        if (economyCount > 0 && ecoCapacity > 0)
        {
            int bookedEco = scheduledFlightId.HasValue
                ? await _purchaseRepo.GetBookedSeatsByClassAsync(scheduledFlightId.Value, "Economy")
                : 0;
            _logger.LogInformation("IsFlightAvailableAsync: bookedEco={B} requested={R} cap={C}", bookedEco, economyCount, ecoCapacity);
            if (economyCount + bookedEco > ecoCapacity) return false;
        }

        return true;
    }

    public async Task<List<string>> CheckPassengerDuplicatesAsync(
        string routeCode, DateOnly flightDate, IEnumerable<PassengerCheckInfo> passengers)
    {
        var flightDateTime     = flightDate.ToDateTime(TimeOnly.MinValue);
        int? scheduledFlightId = _routeCreationService.FindExistingScheduledFlight(routeCode, flightDateTime);

        if (!scheduledFlightId.HasValue)
            return [];

        var existing = await _purchaseRepo.GetPassengerIdentitiesOnFlightAsync(scheduledFlightId.Value);

        return passengers
            .Where(p => existing.Any(e =>
                string.Equals(e.FullName, $"{p.FirstName.Trim()} {p.LastName.Trim()}", StringComparison.OrdinalIgnoreCase) &&
                e.BirthDate.HasValue && DateOnly.FromDateTime(e.BirthDate.Value) == p.BirthDate &&
                string.Equals(e.PassportCountry.Trim(), p.PassportCountry.Trim(), StringComparison.OrdinalIgnoreCase)))
            .Select(p => $"{p.FirstName} {p.LastName}")
            .ToList();
    }

    private async Task<string> GenerateUniqueReservationCodeAsync()
    {
        string code;
        do { code = _codeGenerator.GenerateReservationCode(); }
        while (await _purchaseRepo.ReservationCodeExistsAsync(code));
        return code;
    }

    private async Task<string> GenerateUniqueInvoiceNumberAsync()
    {
        string number;
        do { number = _codeGenerator.GenerateInvoiceNumber(); }
        while (await _purchaseRepo.InvoiceNumberExistsAsync(number));
        return number;
    }

    private static void ValidatePassengers(List<PassengerInfo> passengers)
    {
        if (passengers.Count == 0)
            throw new PassengerDataException("Passengers", "la lista de pasajeros está vacía");

        foreach (var p in passengers)
        {
            if (string.IsNullOrWhiteSpace(p.FirstName))
                throw new PassengerDataException("FirstName",       "el nombre no puede estar vacío");

            if (string.IsNullOrWhiteSpace(p.LastName))
                throw new PassengerDataException("LastName",        "el apellido no puede estar vacío");

            if (string.IsNullOrWhiteSpace(p.PassportCountry))
                throw new PassengerDataException("PassportCountry", "el país del pasaporte no puede estar vacío");

            if (p.BirthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new PassengerDataException("BirthDate",       "la fecha de nacimiento debe ser anterior a hoy");
        }

        var seen = new HashSet<string>();
        foreach (var p in passengers)
        {
            string key = $"{p.FirstName.Trim().ToLowerInvariant()}|{p.LastName.Trim().ToLowerInvariant()}|{p.BirthDate}|{p.PassportCountry.Trim().ToLowerInvariant()}";
            if (!seen.Add(key))
                throw new PassengerDataException("Pasajeros",
                    $"'{p.FirstName} {p.LastName}' aparece más de una vez en la compra con la misma fecha de nacimiento y país.");
        }
    }
}