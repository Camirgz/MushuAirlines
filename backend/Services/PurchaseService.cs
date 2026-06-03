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
    private readonly RouteCreationService _routeCreationService;

    public PurchaseService(
        IPassengerRepository passengerRepo,
        IItineraryRepository itineraryRepo,
        IPurchaseRepository  purchaseRepo,
        ICodeGenerator       codeGenerator,
        IPricingCalculator   pricingCalculator,
        RouteCreationService routeCreationService)
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

        RouteCreationModel route;
        try
        {
            route = _routeCreationService.GetRouteByCode(request.Flight.RouteCode);
        }
        catch
        {
            throw new InvalidFlightDateException(
                request.Flight.FlightDate,
                request.Flight.RouteCode,
                "la ruta no existe o no está disponible");
        }

        int scheduledFlightId = _routeCreationService.GetOrCreateScheduledFlight(
            request.Flight.RouteCode,
            request.Flight.FlightDate.ToDateTime(TimeOnly.MinValue));

        // Check capacity as a whole — no per-seat numbers on the client side
        bool hasSeats = await _purchaseRepo.HasAvailableSeatsAsync(
            scheduledFlightId, request.SeatSelections.Count);

        if (!hasSeats)
            throw new SeatUnavailableException(scheduledFlightId);

        // Auto-assign sequential seat numbers from the next available slot
        var assignedSeatNumbers = await _purchaseRepo.GetNextAvailableSeatNumbersAsync(
            scheduledFlightId, request.SeatSelections.Count);

        var totals = _pricingCalculator.Calculate(
            request.SeatSelections,
            route.PriceEconomy,
            route.PriceFirstClass);

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

        var tickets = new List<TicketSummary>(request.SeatSelections.Count);
        for (int seatIdx = 0; seatIdx < request.SeatSelections.Count; seatIdx++)
        {
            var seat         = request.SeatSelections[seatIdx];
            int passengerId  = passengerIdMap[seat.PassengerIndex];
            int seatNumber   = assignedSeatNumbers[seatIdx];

            await _purchaseRepo.CreateTicketAsync(scheduledFlightId, passengerId, seatNumber);

            var passenger = request.Passengers[seat.PassengerIndex];
            tickets.Add(new TicketSummary
            {
                PassengerFullName = $"{passenger.FirstName} {passenger.LastName}",
                SeatNumber        = seatNumber.ToString(),
                SeatClass         = seat.SeatClass
            });
        }

        await _purchaseRepo.LinkItineraryToScheduledFlightAsync(bookingCode, scheduledFlightId);

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
        var flightDateTime    = flightDate.ToDateTime(TimeOnly.MinValue);
        int? scheduledFlightId = _routeCreationService.FindExistingScheduledFlight(routeCode, flightDateTime);

        if (!scheduledFlightId.HasValue)
            return true; // No scheduled flight created yet → fully available

        return await _purchaseRepo.HasAvailableSeatsAsync(scheduledFlightId.Value, requestedCount);
    }

    private async Task<int> ResolvePassengerAsync(PassengerInfo passenger)
    {
        int? existing = await _passengerRepo.FindPassengerByDocumentAsync(passenger.PassportNumber);

        return existing ?? await _passengerRepo.CreatePassengerAsync(passenger);
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

            if (string.IsNullOrWhiteSpace(p.PassportNumber))
                throw new PassengerDataException("PassportNumber",  "el número de pasaporte no puede estar vacío");

            if (string.IsNullOrWhiteSpace(p.PassportCountry))
                throw new PassengerDataException("PassportCountry", "el país del pasaporte no puede estar vacío");

            if (string.IsNullOrWhiteSpace(p.Gender))
                throw new PassengerDataException("Gender",          "el género no puede estar vacío");

            if (p.BirthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new PassengerDataException("BirthDate",       "la fecha de nacimiento debe ser anterior a hoy");
        }
    }
}
