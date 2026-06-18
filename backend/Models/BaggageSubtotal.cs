namespace backend.Model;

public class BaggageSubtotal
{
    public BaggageType Type { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
