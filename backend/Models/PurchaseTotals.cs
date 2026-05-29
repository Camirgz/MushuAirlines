namespace backend.Model;

public class PurchaseTotals
{
    public decimal TotalPaid { get; set; }
    public int TotalSeats { get; set; }
    public List<SeatClassSubtotal> DetailByClass { get; set; } = [];
}

public class SeatClassSubtotal
{
    public string SeatClass { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Subtotal { get; set; }
}
