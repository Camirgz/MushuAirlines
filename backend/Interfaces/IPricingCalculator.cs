using backend.Model;

namespace backend.Interfaces;

public interface IPricingCalculator
{
    PurchaseTotals Calculate(
        IEnumerable<SeatSelection> seats,
        decimal economyPrice,
        decimal firstClassPrice,
        BaggageInfo baggage,
        decimal handBagPrice,
        decimal bagPrice,
        decimal bagMultiplier);
}
