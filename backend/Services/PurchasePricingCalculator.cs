using backend.Interfaces;
using backend.Model;

namespace backend.Services;

public class PurchasePricingCalculator : IPricingCalculator
{
    public PurchaseTotals Calculate(
        IEnumerable<SeatSelection> seats,
        decimal economyPrice,
        decimal firstClassPrice)
    {
        var seatList = seats.ToList();

        var detailByClass = seatList
            .GroupBy(s => s.SeatClass)
            .Select(group =>
            {
                decimal unitPrice = group.Key switch
                {
                    "Economy"    => economyPrice,
                    "FirstClass" => firstClassPrice,
                    _ => throw new ArgumentException(
                             $"Clase de asiento desconocida: '{group.Key}'. " +
                             "Valores válidos: 'Economy', 'FirstClass'.")
                };

                int seatcount = group.Count();

                return new SeatClassSubtotal
                {
                    SeatClass = group.Key,
                    SeatCount = seatcount,
                    Subtotal  = seatcount * unitPrice
                };
            })
            .ToList();

        return new PurchaseTotals
        {
            TotalPaid    = detailByClass.Sum(d => d.Subtotal),
            TotalSeats   = detailByClass.Sum(d => d.SeatCount),
            DetailByClass = detailByClass
        };
    }
}
