using backend.Exceptions;
using backend.Interfaces;
using backend.Model;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPassengerRepository     _passengerRepo;
    private readonly IPurchaseRepository      _purchaseRepo;
    private readonly ICodeGenerator           _codeGenerator;
    private readonly IPricingCalculator       _pricingCalculator;
    private readonly IRouteCreationService    _routeCreationService;
    private readonly IExternalAirlinesService _externalAirlinesService;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseService(
        IPassengerRepository     passengerRepo,
        IPurchaseRepository      purchaseRepo,
        ICodeGenerator           codeGenerator,
        IPricingCalculator       pricingCalculator,
        IRouteCreationService    routeCreationService,
        IExternalAirlinesService externalAirlinesService,
        ILogger<PurchaseService> logger)
    {
        _passengerRepo           = passengerRepo;
        _purchaseRepo            = purchaseRepo;
        _codeGenerator           = codeGenerator;
        _pricingCalculator       = pricingCalculator;
        _routeCreationService    = routeCreationService;
        _externalAirlinesService = externalAirlinesService;
        _logger                  = logger;
    }

    public async Task<PurchaseResponseModel> CreatePurchaseAsync(PurchaseRequestModel request)
    {
        ValidatePassengers(request.Passengers);

        bool leg1IsExternal = request.ExternalFlight  != null;
        bool leg2IsExternal = request.ExternalFlight2 != null;
        bool hasLeg2        = request.Flight2 != null || request.ExternalFlight2 != null;

        // ── Resolve leg 1 ────────────────────────────────────────────────
        int?   scheduledFlightId1 = null;
        string airlineName1       = "Mushu Airlines";

        RouteCreationModel? route1 = null;

        if (!leg1IsExternal)
        {
            if (request.Flight == null)
                throw new InvalidFlightDateException(DateOnly.MinValue, "", "se requiere Flight o ExternalFlight para el primer tramo");

            try { route1 = _routeCreationService.GetRouteByCode(request.Flight.RouteCode); }
            catch
            {
                throw new InvalidFlightDateException(
                    request.Flight.FlightDate, request.Flight.RouteCode,
                    "la ruta no existe o no está disponible");
            }

            scheduledFlightId1 = _routeCreationService.GetOrCreateScheduledFlight(
                request.Flight.RouteCode,
                request.Flight.FlightDate.ToDateTime(TimeOnly.MinValue));
        }
        else
        {
            airlineName1 = request.ExternalFlight!.AirlineName;
        }

        // ── Resolve leg 2 ─────────────────────────────────────────────────
        int?   scheduledFlightId2 = null;
        string? airlineName2      = null;

        RouteCreationModel? route2 = null;

        if (hasLeg2)
        {
            if (!leg2IsExternal)
            {
                try { route2 = _routeCreationService.GetRouteByCode(request.Flight2!.RouteCode); }
                catch
                {
                    throw new InvalidFlightDateException(
                        request.Flight2!.FlightDate, request.Flight2.RouteCode,
                        "la ruta del segundo tramo no existe o no está disponible");
                }

                scheduledFlightId2 = _routeCreationService.GetOrCreateScheduledFlight(
                    request.Flight2!.RouteCode,
                    request.Flight2.FlightDate.ToDateTime(TimeOnly.MinValue));

                airlineName2 = "Mushu Airlines";
            }
            else
            {
                airlineName2 = request.ExternalFlight2!.AirlineName;
            }
        }

        // ── Seat availability (per class, only for internal legs) ─────────
        int firstClassCount = request.SeatSelections.Count(s => s.SeatClass == SeatClass.FirstClass);
        int economyCount    = request.SeatSelections.Count(s => s.SeatClass == SeatClass.Economy);
        var seatCount       = request.SeatSelections.Count;

        if (!leg1IsExternal && request.Flight != null)
        {
            bool avail = await CheckPerClassAvailabilityAsync(
                request.Flight.RouteCode, request.Flight.FlightDate, firstClassCount, economyCount);
            if (!avail) throw new SeatUnavailableException(scheduledFlightId1!.Value);
        }

        if (hasLeg2 && !leg2IsExternal && request.Flight2 != null)
        {
            bool avail = await CheckPerClassAvailabilityAsync(
                request.Flight2.RouteCode, request.Flight2.FlightDate, firstClassCount, economyCount);
            if (!avail) throw new SeatUnavailableException(scheduledFlightId2!.Value);
        }

        // ── Assign seat numbers (only for internal legs) ──────────────────
        List<int> assignedSeatNumbers1 = leg1IsExternal
            ? Enumerable.Range(1, seatCount).ToList()
            : await _purchaseRepo.GetNextAvailableSeatNumbersAsync(scheduledFlightId1!.Value, seatCount);

        List<int>? assignedSeatNumbers2 = null;
        if (hasLeg2)
        {
            assignedSeatNumbers2 = leg2IsExternal
                ? Enumerable.Range(1, seatCount).ToList()
                : await _purchaseRepo.GetNextAvailableSeatNumbersAsync(scheduledFlightId2!.Value, seatCount);
        }

        // ── Pricing ───────────────────────────────────────────────────────
        decimal economyPrice    = GetEconomyPrice(leg1IsExternal,  request, route1)
                                + (hasLeg2 ? GetEconomyPrice2(leg2IsExternal, request, route2) : 0);
        decimal firstClassPrice = GetFirstClassPrice(leg1IsExternal,  request, route1)
                                + (hasLeg2 ? GetFirstClassPrice2(leg2IsExternal, request, route2) : 0);
        decimal handBagPrice    = GetHandBagPrice(leg1IsExternal,  request, route1)
                                + (hasLeg2 ? GetHandBagPrice2(leg2IsExternal, request, route2) : 0);
        decimal bagPrice        = GetBagPrice(leg1IsExternal,  request, route1)
                                + (hasLeg2 ? GetBagPrice2(leg2IsExternal, request, route2) : 0);
        decimal bagMultiplier   = leg1IsExternal ? 1m : route1!.BagMultiplier;

        var totals = _pricingCalculator.Calculate(
            request.SeatSelections,
            request.Passengers,
            economyPrice,
            firstClassPrice,
            handBagPrice,
            bagPrice,
            bagMultiplier);

        // ── Unique codes ──────────────────────────────────────────────────
        var uniqueCodes = await Task.WhenAll(
            GenerateUniqueReservationCodeAsync(),
            GenerateUniqueInvoiceNumberAsync());

        var reservationCode = uniqueCodes[0];
        var invoiceNumber   = uniqueCodes[1];

        // ── Create passengers ─────────────────────────────────────────────
        var passengerIds     = await Task.WhenAll(
            request.Passengers.Select(p => _passengerRepo.CreatePassengerAsync(p)));
        var firstPassengerId = passengerIds[0];
        var purchaseDate     = DateTime.UtcNow;

        // ── Build tickets leg 1 ───────────────────────────────────────────
        var tickets1 = request.SeatSelections
            .Select((seat, i) => new TicketInsertData
            {
                ScheduledFlightId = leg1IsExternal ? 0 : scheduledFlightId1!.Value,
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
                    ScheduledFlightId = leg1IsExternal ? 0 : scheduledFlightId1!.Value,
                    PassengerId       = passengerIds[seat.PassengerIndex],
                    HandBagCount      = bag.HandBagCount,
                    CheckedBagCount   = bag.CheckedBagCount,
                    Subtotal          = bag.Subtotal
                };
            })
            .ToList();

        // ── Build tickets leg 2 ───────────────────────────────────────────
        List<TicketInsertData>?        tickets2       = null;
        List<TicketBaggageInsertData>? ticketBaggage2 = null;

        if (hasLeg2)
        {
            tickets2 = request.SeatSelections
                .Select((seat, i) => new TicketInsertData
                {
                    ScheduledFlightId = leg2IsExternal ? 0 : scheduledFlightId2!.Value,
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
                        ScheduledFlightId = leg2IsExternal ? 0 : scheduledFlightId2!.Value,
                        PassengerId       = passengerIds[seat.PassengerIndex],
                        HandBagCount      = bag.HandBagCount,
                        CheckedBagCount   = bag.CheckedBagCount,
                        Subtotal          = bag.Subtotal
                    };
                })
                .ToList();
        }

        // ── Build external flight insert data ─────────────────────────────
        ExternalFlightInsertData? extFlight1 = leg1IsExternal
            ? ToExternalFlightInsertData(request.ExternalFlight!)
            : null;

        ExternalFlightInsertData? extFlight2 = leg2IsExternal
            ? ToExternalFlightInsertData(request.ExternalFlight2!)
            : null;

        // ── Book with external airline + execute local transaction ────────
        int purchaseId;
        try
        {
            if (leg2IsExternal)
            {
                var extOrder = BuildExternalOrder(
                    request, firstClassCount > 0, ticketBaggage2!);

                Console.WriteLine($"\n[ExternalBooking] Aerolínea : {request.ExternalFlight2!.AirlineName}");
                Console.WriteLine($"[ExternalBooking] GUID búsqueda  : {request.ExternalFlight2!.FlightGUID}");
                Console.WriteLine($"[ExternalBooking] GUID en request : {extOrder.FlightGUID}");

                await _externalAirlinesService.BookExternalFlightAsync(
                    request.ExternalFlight2!.AirlineName, extOrder);
            }

            purchaseId = await _purchaseRepo.ExecutePurchaseTransactionAsync(new PurchaseTransactionData
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
                Details         = totals.DetailByClass,
                BaggageDetails  = totals.BaggageDetails,
                ScheduledId1    = scheduledFlightId1,
                ExternalFlight1 = extFlight1,
                AirlineName1    = airlineName1,
                Tickets1        = tickets1,
                TicketBaggage1  = ticketBaggage1,
                ScheduledId2    = scheduledFlightId2,
                ExternalFlight2 = extFlight2,
                AirlineName2    = airlineName2,
                Tickets2        = tickets2,
                TicketBaggage2  = ticketBaggage2,
            });
        }
        catch
        {
            await _passengerRepo.DeletePassengersAsync(passengerIds);
            throw;
        }

        var leg1Id = leg1IsExternal ? request.ExternalFlight!.FlightGUID : scheduledFlightId1!.Value.ToString();
        var leg2Id = hasLeg2
            ? (leg2IsExternal ? request.ExternalFlight2!.FlightGUID : scheduledFlightId2!.Value.ToString())
            : null;

        var tickets = GenerateTicketSummaries(
            request.SeatSelections,
            request.Passengers,
            assignedSeatNumbers1,
            assignedSeatNumbers2,
            hasLeg2,
            leg1Id,
            leg2Id);

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

    public Task<bool> IsFlightAvailableAsync(string routeCode, DateOnly flightDate, int firstClassCount, int economyCount)
        => CheckPerClassAvailabilityAsync(routeCode, flightDate, firstClassCount, economyCount);

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

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<bool> CheckPerClassAvailabilityAsync(
        string routeCode, DateOnly flightDate, int firstClassCount, int economyCount)
    {
        RouteCreationModel route;
        try { route = _routeCreationService.GetRouteByCode(routeCode); }
        catch (Exception ex)
        {
            _logger.LogWarning("CheckPerClassAvailabilityAsync: route '{RouteCode}' not found — {Msg}", routeCode, ex.Message);
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
                "CheckPerClassAvailabilityAsync: route capacities were 0, fell back to aircraft — FC={FC} Eco={Eco}",
                fcCapacity, ecoCapacity);
        }

        var flightDateTime     = flightDate.ToDateTime(TimeOnly.MinValue);
        int? scheduledFlightId = _routeCreationService.FindExistingScheduledFlight(routeCode, flightDateTime);

        if (firstClassCount > 0 && fcCapacity > 0)
        {
            int bookedFC = scheduledFlightId.HasValue
                ? await _purchaseRepo.GetBookedSeatsByClassAsync(scheduledFlightId.Value, "FirstClass")
                : 0;
            if (firstClassCount + bookedFC > fcCapacity) return false;
        }

        if (economyCount > 0 && ecoCapacity > 0)
        {
            int bookedEco = scheduledFlightId.HasValue
                ? await _purchaseRepo.GetBookedSeatsByClassAsync(scheduledFlightId.Value, "Economy")
                : 0;
            if (economyCount + bookedEco > ecoCapacity) return false;
        }

        return true;
    }

    private static readonly Dictionary<string, string> _iso2To3 = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AD"] = "AND", ["AE"] = "ARE", ["AF"] = "AFG", ["AG"] = "ATG", ["AL"] = "ALB",
        ["AM"] = "ARM", ["AO"] = "AGO", ["AR"] = "ARG", ["AT"] = "AUT", ["AU"] = "AUS",
        ["AZ"] = "AZE", ["BA"] = "BIH", ["BB"] = "BRB", ["BD"] = "BGD", ["BE"] = "BEL",
        ["BF"] = "BFA", ["BG"] = "BGR", ["BH"] = "BHR", ["BI"] = "BDI", ["BJ"] = "BEN",
        ["BN"] = "BRN", ["BO"] = "BOL", ["BR"] = "BRA", ["BS"] = "BHS", ["BT"] = "BTN",
        ["BW"] = "BWA", ["BY"] = "BLR", ["BZ"] = "BLZ", ["CA"] = "CAN", ["CD"] = "COD",
        ["CF"] = "CAF", ["CG"] = "COG", ["CH"] = "CHE", ["CI"] = "CIV", ["CL"] = "CHL",
        ["CM"] = "CMR", ["CN"] = "CHN", ["CO"] = "COL", ["CR"] = "CRI", ["CU"] = "CUB",
        ["CV"] = "CPV", ["CY"] = "CYP", ["CZ"] = "CZE", ["DE"] = "DEU", ["DJ"] = "DJI",
        ["DK"] = "DNK", ["DM"] = "DMA", ["DO"] = "DOM", ["DZ"] = "DZA", ["EC"] = "ECU",
        ["EE"] = "EST", ["EG"] = "EGY", ["ER"] = "ERI", ["ES"] = "ESP", ["ET"] = "ETH",
        ["FI"] = "FIN", ["FJ"] = "FJI", ["FM"] = "FSM", ["FR"] = "FRA", ["GA"] = "GAB",
        ["GB"] = "GBR", ["GD"] = "GRD", ["GE"] = "GEO", ["GH"] = "GHA", ["GM"] = "GMB",
        ["GN"] = "GIN", ["GQ"] = "GNQ", ["GR"] = "GRC", ["GT"] = "GTM", ["GW"] = "GNB",
        ["GY"] = "GUY", ["HN"] = "HND", ["HR"] = "HRV", ["HT"] = "HTI", ["HU"] = "HUN",
        ["ID"] = "IDN", ["IE"] = "IRL", ["IL"] = "ISR", ["IN"] = "IND", ["IQ"] = "IRQ",
        ["IR"] = "IRN", ["IS"] = "ISL", ["IT"] = "ITA", ["JM"] = "JAM", ["JO"] = "JOR",
        ["JP"] = "JPN", ["KE"] = "KEN", ["KG"] = "KGZ", ["KH"] = "KHM", ["KI"] = "KIR",
        ["KM"] = "COM", ["KN"] = "KNA", ["KP"] = "PRK", ["KR"] = "KOR", ["KW"] = "KWT",
        ["KZ"] = "KAZ", ["LA"] = "LAO", ["LB"] = "LBN", ["LC"] = "LCA", ["LI"] = "LIE",
        ["LK"] = "LKA", ["LR"] = "LBR", ["LS"] = "LSO", ["LT"] = "LTU", ["LU"] = "LUX",
        ["LV"] = "LVA", ["LY"] = "LBY", ["MA"] = "MAR", ["MC"] = "MCO", ["MD"] = "MDA",
        ["ME"] = "MNE", ["MG"] = "MDG", ["MH"] = "MHL", ["MK"] = "MKD", ["ML"] = "MLI",
        ["MM"] = "MMR", ["MN"] = "MNG", ["MR"] = "MRT", ["MT"] = "MLT", ["MU"] = "MUS",
        ["MV"] = "MDV", ["MW"] = "MWI", ["MX"] = "MEX", ["MY"] = "MYS", ["MZ"] = "MOZ",
        ["NA"] = "NAM", ["NE"] = "NER", ["NG"] = "NGA", ["NI"] = "NIC", ["NL"] = "NLD",
        ["NO"] = "NOR", ["NP"] = "NPL", ["NR"] = "NRU", ["NZ"] = "NZL", ["OM"] = "OMN",
        ["PA"] = "PAN", ["PE"] = "PER", ["PG"] = "PNG", ["PH"] = "PHL", ["PK"] = "PAK",
        ["PL"] = "POL", ["PT"] = "PRT", ["PW"] = "PLW", ["PY"] = "PRY", ["QA"] = "QAT",
        ["RO"] = "ROU", ["RS"] = "SRB", ["RU"] = "RUS", ["RW"] = "RWA", ["SA"] = "SAU",
        ["SB"] = "SLB", ["SC"] = "SYC", ["SD"] = "SDN", ["SE"] = "SWE", ["SG"] = "SGP",
        ["SI"] = "SVN", ["SK"] = "SVK", ["SL"] = "SLE", ["SM"] = "SMR", ["SN"] = "SEN",
        ["SO"] = "SOM", ["SR"] = "SUR", ["SS"] = "SSD", ["ST"] = "STP", ["SV"] = "SLV",
        ["SY"] = "SYR", ["SZ"] = "SWZ", ["TD"] = "TCD", ["TG"] = "TGO", ["TH"] = "THA",
        ["TJ"] = "TJK", ["TL"] = "TLS", ["TM"] = "TKM", ["TN"] = "TUN", ["TO"] = "TON",
        ["TR"] = "TUR", ["TT"] = "TTO", ["TV"] = "TUV", ["TZ"] = "TZA", ["UA"] = "UKR",
        ["UG"] = "UGA", ["US"] = "USA", ["UY"] = "URY", ["UZ"] = "UZB", ["VA"] = "VAT",
        ["VC"] = "VCT", ["VE"] = "VEN", ["VN"] = "VNM", ["VU"] = "VUT", ["WS"] = "WSM",
        ["YE"] = "YEM", ["ZA"] = "ZAF", ["ZM"] = "ZMB", ["ZW"] = "ZWE",
    };

    private static string ToIso3(string iso2) =>
        _iso2To3.TryGetValue(iso2 ?? "", out var iso3) ? iso3 : (iso2 ?? "");

    private static ExternalOrderRequest BuildExternalOrder(
        PurchaseRequestModel          request,
        bool                          firstClass,
        List<TicketBaggageInsertData> baggage2)
    {
        var buyer = request.Passengers[0];

        var passengers = request.Passengers.Select((p, i) => new ExternalOrderPassenger
        {
            CarryOn                = baggage2[i].HandBagCount > 0,
            Checked                = baggage2[i].CheckedBagCount,
            Passport               = "000000000",
            PassportExpirationDate = "2030-01-01",
            PassportCountry        = ToIso3(p.PassportCountry),
            FirstName              = p.FirstName,
            LastName               = p.LastName,
            LastName2              = null,
            Gender                 = p.Gender switch
            {
                Gender.Male   => "M",
                Gender.Female => "F",
                _             => "O"
            },
            BirthDate = p.BirthDate.ToString("yyyy-MM-dd")
        }).ToList();

        return new ExternalOrderRequest
        {
            FlightGUID = request.ExternalFlight2!.FlightGUID,
            FirstClass = firstClass,
            Passengers = passengers,
            Buyer = new ExternalOrderBuyer
            {
                Nationality = ToIso3(buyer.PassportCountry),
                FirstName   = buyer.FirstName,
                LastName    = buyer.LastName,
                LastName2   = null,
                PhoneNumber = buyer.Phone,
                Email       = request.Payment.ContactEmail
            },
            Payment = new ExternalOrderPayment
            {
                CardNumber     = "4111111111111111",
                CardExpiration = "2030-01",
                Cvv            = "0000",
                CardHolderName = $"{buyer.FirstName} {buyer.LastName}"
            }
        };
    }

    private static ExternalFlightInsertData ToExternalFlightInsertData(ExternalFlightSelection ext) =>
        new()
        {
            Code               = ext.FlightGUID,
            AirlineName        = ext.AirlineName,
            DepartureTime      = DateTime.Parse(ext.DepartureTime),
            ArrivalTime        = DateTime.Parse(ext.ArrivalTime),
            OriginAirport      = ext.OriginAirport,
            DestinationAirport = ext.DestinationAirport,
            PriceEconomy       = ext.TouristPrice,
            PriceFirstClass    = ext.FirstClassPrice,
            HandBagPrice       = ext.CarryOnPrice,
            BagPrice           = ext.CheckedPrice,
        };

    private static decimal GetEconomyPrice(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight!.TouristPrice : route!.PriceEconomy;

    private static decimal GetEconomyPrice2(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight2!.TouristPrice : route!.PriceEconomy;

    private static decimal GetFirstClassPrice(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight!.FirstClassPrice : route!.PriceFirstClass;

    private static decimal GetFirstClassPrice2(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight2!.FirstClassPrice : route!.PriceFirstClass;

    private static decimal GetHandBagPrice(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight!.CarryOnPrice : route!.HandBagPrice;

    private static decimal GetHandBagPrice2(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight2!.CarryOnPrice : route!.HandBagPrice;

    private static decimal GetBagPrice(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight!.CheckedPrice : route!.BagPrice;

    private static decimal GetBagPrice2(bool isExternal, PurchaseRequestModel r, RouteCreationModel? route)
        => isExternal ? r.ExternalFlight2!.CheckedPrice : route!.BagPrice;

    private static List<TicketSummary> GenerateTicketSummaries(
        List<SeatSelection> seatSelections,
        List<PassengerInfo> passengers,
        List<int> seatNumbers1,
        List<int>? seatNumbers2,
        bool hasLeg2,
        string flightId1,
        string? flightId2)
    {
        var tickets = new List<TicketSummary>();

        tickets.AddRange(seatSelections.Select((seat, i) =>
        {
            var passenger = passengers[seat.PassengerIndex];
            return new TicketSummary
            {
                PassengerFullName = $"{passenger.FirstName} {passenger.LastName}",
                SeatNumber        = seatNumbers1[i].ToString(),
                SeatClass         = seat.SeatClass,
                FlightNumber      = flightId1
            };
        }));

        if (hasLeg2 && seatNumbers2 != null && flightId2 != null)
        {
            tickets.AddRange(seatSelections.Select((seat, i) =>
            {
                var passenger = passengers[seat.PassengerIndex];
                return new TicketSummary
                {
                    PassengerFullName = $"{passenger.FirstName} {passenger.LastName}",
                    SeatNumber        = seatNumbers2[i].ToString(),
                    SeatClass         = seat.SeatClass,
                    FlightNumber      = flightId2
                };
            }));
        }

        return tickets;
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
