using backend.Model;

namespace backend.Interfaces;

public interface IPricingCalculator
{
    PurchaseTotals Calculate(
        IEnumerable<SeatSelection> seats,
        IEnumerable<PassengerInfo> passengers,
        decimal economyPrice,
        decimal firstClassPrice,
        decimal handBagPrice,
        decimal bagPrice,
        decimal bagMultiplier);
}
