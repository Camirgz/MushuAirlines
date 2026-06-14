namespace backend.Model;

public class PurchaseTotals
{
    public decimal TotalPaid { get; set; }
    public int TotalSeats { get; set; }
    public List<SeatClassSubtotal> DetailByClass { get; set; } = [];
    public List<BaggageSubtotal> BaggageDetails { get; set; } = [];
    public List<PassengerBaggageSubtotal> PassengerBaggageDetails { get; set; } = [];
    public int HandBaggageCount { get; set; }
    public decimal HandBaggageSubtotal { get; set; }
    public int CheckedBaggageCount { get; set; }
    public decimal CheckedBaggageSubtotal { get; set; }
}

public class PassengerBaggageSubtotal
{
    public int PassengerIndex   { get; set; }
    public int HandBagCount     { get; set; }
    public int CheckedBagCount  { get; set; }
    public decimal HandSubtotal     { get; set; }
    public decimal CheckedSubtotal  { get; set; }
    public decimal Subtotal => HandSubtotal + CheckedSubtotal;
}

public class SeatClassSubtotal
{
    public string SeatClass { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public decimal Subtotal { get; set; }
     // derivated
    public decimal PricePerSeat
    {
        get
        {
            if (SeatCount > 0)
                return Subtotal / SeatCount;
            return 0;
        }
    }    
}
