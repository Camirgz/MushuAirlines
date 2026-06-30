namespace backend.Model;

public class PurchaseTransactionData
{
    public required PurchaseRecord                 Record              { get; init; }
    public required List<SeatClassSubtotal>        Details             { get; init; }
    public required List<BaggageSubtotal>          BaggageDetails      { get; init; }

    // Leg 1 — exactly one of ScheduledId1 or ExternalFlight1 must be set
    public          int?                           ScheduledId1        { get; init; }
    public          ExternalFlightInsertData?      ExternalFlight1     { get; init; }
    public required string                         AirlineName1        { get; init; }
    public required List<TicketInsertData>         Tickets1            { get; init; }
    public required List<TicketBaggageInsertData>  TicketBaggage1      { get; init; }

    // Leg 2 — optional stopover
    public          int?                           ScheduledId2        { get; init; }
    public          ExternalFlightInsertData?      ExternalFlight2     { get; init; }
    public          string?                        AirlineName2        { get; init; }
    public          List<TicketInsertData>?        Tickets2            { get; init; }
    public          List<TicketBaggageInsertData>? TicketBaggage2      { get; init; }
}

public class ExternalFlightInsertData
{
    // Code = FlightGUID, used as PK in the existing ExternalFlight table
    public required string   Code               { get; init; } // VARCHAR(100), maps to ExternalFlight.Code
    public required string   AirlineName        { get; init; }
    public required DateTime DepartureTime      { get; init; }
    public required DateTime ArrivalTime        { get; init; }
    public required string   OriginAirport      { get; init; }
    public required string   DestinationAirport { get; init; }
    public required decimal  PriceEconomy       { get; init; }
    public required decimal  PriceFirstClass    { get; init; }
    public required decimal  HandBagPrice       { get; init; }
    public required decimal  BagPrice           { get; init; }
}
