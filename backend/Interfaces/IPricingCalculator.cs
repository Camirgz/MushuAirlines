using backend.Model;

namespace backend.Interfaces;

public interface IPricingCalculator
{
    PurchaseTotals Calculate(
        IEnumerable<SeatSelection> seats,
        decimal economyPrice,
        decimal firstClassPrice);
}
