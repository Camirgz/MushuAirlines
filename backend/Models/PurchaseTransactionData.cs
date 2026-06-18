namespace backend.Model;

public class PurchaseTransactionData
{
    public required PurchaseRecord                 Record         { get; init; }
    public required List<SeatClassSubtotal>        Details        { get; init; }
    public required List<BaggageSubtotal>          BaggageDetails { get; init; }
    public required List<TicketInsertData>         Tickets1       { get; init; }
    public required List<TicketBaggageInsertData>  TicketBaggage1 { get; init; }
    public required int                            ScheduledId1   { get; init; }
    public          List<TicketInsertData>?        Tickets2       { get; init; }
    public          List<TicketBaggageInsertData>? TicketBaggage2 { get; init; }
    public          int?                           ScheduledId2   { get; init; }
}
