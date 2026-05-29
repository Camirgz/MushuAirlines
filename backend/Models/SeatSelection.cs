namespace backend.Model;

// SeatClass expected values: "Economy", "FirstClass"
public class SeatSelection
{
    public int PassengerIndex { get; set; }
    public string SeatClass { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
}
