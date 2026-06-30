namespace backend.Model;

public class TicketInsertData
{
    public int    ScheduledFlightId { get; init; }
    public int    PassengerId       { get; init; }
    public int    SeatNumber        { get; init; }
    public string SeatClass         { get; set; } = string.Empty;
}

public class TicketBaggageInsertData
{
    public int     ScheduledFlightId { get; init; }
    public int     PassengerId       { get; init; }
    public int     HandBagCount      { get; init; }
    public int     CheckedBagCount   { get; init; }
    public decimal Subtotal          { get; init; }
}

public class ExternalTicketBaggageInsertData
{
    public int     ExternalFlightId { get; init; }
    public int     PassengerId      { get; init; }
    public int     HandBagCount     { get; init; }
    public int     CheckedBagCount  { get; init; }
    public decimal Subtotal         { get; init; }
}
