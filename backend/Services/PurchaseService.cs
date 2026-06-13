using backend.Exceptions;
using backend.Interfaces;
using backend.Model;

namespace backend.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPassengerRepository _passengerRepo;
    private readonly IItineraryRepository _itineraryRepo;
    private readonly IPurchaseRepository  _purchaseRepo;
    private readonly ICodeGenerator       _codeGenerator;
    private readonly IPricingCalculator   _pricingCalculator;
    private readonly IRouteCreationService _routeCreationService;

    public PurchaseService(
        IPassengerRepository  passengerRepo,
        IItineraryRepository  itineraryRepo,
        IPurchaseRepository   purchaseRepo,
        ICodeGenerator        codeGenerator,
        IPricingCalculator    pricingCalculator,
        IRouteCreationService routeCreationService)
    {
        _passengerRepo        = passengerRepo;
        _itineraryRepo        = itineraryRepo;
        _purchaseRepo         = purchaseRepo;
        _codeGenerator        = codeGenerator;
        _pricingCalculator    = pricingCalculator;
        _routeCreationService = routeCreationService;
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

        bool hasSeats1 = await _purchaseRepo.HasAvailableSeatsAsync(
            scheduledFlightId1, request.SeatSelections.Count);
        if (!hasSeats1)
            throw new SeatUnavailableException(scheduledFlightId1);

        if (isStopover)
        {
            bool hasSeats2 = await _purchaseRepo.HasAvailableSeatsAsync(
                scheduledFlightId2, request.SeatSelections.Count);
            if (!hasSeats2)
                throw new SeatUnavailableException(scheduledFlightId2);
        }

        var assignedSeatNumbers1 = await _purchaseRepo.GetNextAvailableSeatNumbersAsync(
            scheduledFlightId1, request.SeatSelections.Count);

        List<int>? assignedSeatNumbers2 = null;
        if (isStopover)
        {
            assignedSeatNumbers2 = await _purchaseRepo.GetNextAvailableSeatNumbersAsync(
                scheduledFlightId2, request.SeatSelections.Count);
        }

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

        string reservationCode;
        do
        {
            reservationCode = _codeGenerator.GenerateReservationCode();
        } while (await _purchaseRepo.ReservationCodeExistsAsync(reservationCode));

        string invoiceNumber = _codeGenerator.GenerateInvoiceNumber();

        var passengerIdMap = new Dictionary<int, int>(request.Passengers.Count);
        for (int i = 0; i < request.Passengers.Count; i++)
        {
            passengerIdMap[i] = await ResolvePassengerAsync(request.Passengers[i]);
        }

        int firstPassengerId = passengerIdMap[0];
        int bookingCode      = await _itineraryRepo.CreateItineraryAsync(firstPassengerId);

        var purchaseDate = DateTime.UtcNow;
        int purchaseId   = await _purchaseRepo.CreatePurchaseAsync(new PurchaseRecord
        {
            PassengerId     = firstPassengerId,
            BookingCode     = bookingCode,
            ReservationCode = reservationCode,
            InvoiceNumber   = invoiceNumber,
            PaymentMethod   = request.Payment.Method,
            Email           = request.Payment.ContactEmail,
            TotalPaid       = totals.TotalPaid,
            TotalSeats      = totals.TotalSeats,
            PurchaseDate    = purchaseDate
        });

        foreach (var detail in totals.DetailByClass)
            await _purchaseRepo.CreatePurchaseDetailAsync(
                purchaseId, detail.SeatClass, detail.SeatCount, detail.Subtotal);

        foreach (var baggageDetail in totals.BaggageDetails)
            await _purchaseRepo.CreatePurchaseBaggageDetailAsync(
                purchaseId, baggageDetail.Type, baggageDetail.Quantity, baggageDetail.UnitPrice, baggageDetail.Subtotal);

        var tickets = new List<TicketSummary>(request.SeatSelections.Count);
        for (int seatIdx = 0; seatIdx < request.SeatSelections.Count; seatIdx++)
        {
            var seat            = request.SeatSelections[seatIdx];
            int passengerId     = passengerIdMap[seat.PassengerIndex];
            int seatNumber      = assignedSeatNumbers1[seatIdx];
            var passengerBag    = totals.PassengerBaggageDetails[seat.PassengerIndex];

            await _purchaseRepo.CreateTicketAsync(scheduledFlightId1, passengerId, seatNumber);
            await _purchaseRepo.CreateTicketBaggageAsync(
                scheduledFlightId1, passengerId, bookingCode,
                passengerBag.HandBagCount, passengerBag.CheckedBagCount, passengerBag.Subtotal);

            var passenger = request.Passengers[seat.PassengerIndex];
            tickets.Add(new TicketSummary
            {
                PassengerFullName = $"{passenger.FirstName} {passenger.LastName}",
                SeatNumber        = seatNumber.ToString(),
                SeatClass         = seat.SeatClass
            });
        }

        await _purchaseRepo.LinkItineraryToScheduledFlightAsync(bookingCode, scheduledFlightId1);

        if (isStopover)
        {
            for (int seatIdx = 0; seatIdx < request.SeatSelections.Count; seatIdx++)
            {
                var seat         = request.SeatSelections[seatIdx];
                int passengerId  = passengerIdMap[seat.PassengerIndex];
                int seatNumber   = assignedSeatNumbers2![seatIdx];
                var passengerBag = totals.PassengerBaggageDetails[seat.PassengerIndex];

                await _purchaseRepo.CreateTicketAsync(scheduledFlightId2, passengerId, seatNumber);
                await _purchaseRepo.CreateTicketBaggageAsync(
                    scheduledFlightId2, passengerId, bookingCode,
                    passengerBag.HandBagCount, passengerBag.CheckedBagCount, passengerBag.Subtotal);
            }

            await _purchaseRepo.LinkItineraryToScheduledFlightAsync(bookingCode, scheduledFlightId2);
        }

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

    public async Task<bool> IsFlightAvailableAsync(string routeCode, DateOnly flightDate, int requestedCount)
    {
        RouteCreationModel route;
        try { route = _routeCreationService.GetRouteByCode(routeCode); }
        catch { return true; } 


        int capacity = route.EconomyClassCapacity + route.FirstClassCapacity;
        if (capacity == 0)
            capacity = await _purchaseRepo.GetAircraftCapacityByTypeAsync(route.AircraftTypeId);
        if (capacity == 0) return true; 
        if (requestedCount > capacity) return false;

        var flightDateTime     = flightDate.ToDateTime(TimeOnly.MinValue);
        int? scheduledFlightId = _routeCreationService.FindExistingScheduledFlight(routeCode, flightDateTime);

        if (!scheduledFlightId.HasValue)
            return true;

        int bookedSeats = await _purchaseRepo.GetBookedSeatsAsync(scheduledFlightId.Value);
        return requestedCount + bookedSeats <= capacity;
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

    private async Task<int> ResolvePassengerAsync(PassengerInfo passenger)
        => await _passengerRepo.CreatePassengerAsync(passenger);

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

            if (string.IsNullOrWhiteSpace(p.Gender))
                throw new PassengerDataException("Gender",          "el género no puede estar vacío");

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
