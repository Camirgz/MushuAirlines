namespace backend.Model;

public class BaggageSubtotal
{
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
