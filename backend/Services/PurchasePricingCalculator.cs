using backend.Interfaces;
using backend.Model;

namespace backend.Services;

public class PurchasePricingCalculator : IPricingCalculator
{
    public PurchaseTotals Calculate(
        IEnumerable<SeatSelection> seats,
        decimal economyPrice,
        decimal firstClassPrice,
        BaggageInfo baggage,
        decimal handBagPrice,
        decimal bagPrice,
        decimal bagMultiplier)
    {
        var seatList = seats.ToList();

        // Calculate seat details by class
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

        // Calculate baggage totals
        decimal handBaggageSubtotal = 0m;
        decimal checkedBaggageSubtotal = 0m;
        var baggageDetails = new List<BaggageSubtotal>();

        if (baggage.HandCount > 0)
        {
            handBaggageSubtotal = baggage.HandCount * handBagPrice;
            baggageDetails.Add(new BaggageSubtotal
            {
                Type = "HandBaggage",
                Quantity = baggage.HandCount,
                UnitPrice = handBagPrice,
                Subtotal = handBaggageSubtotal
            });
        }

        if (baggage.CheckedCount > 0)
        {
            checkedBaggageSubtotal = baggage.CheckedCount * bagPrice * bagMultiplier;
            baggageDetails.Add(new BaggageSubtotal
            {
                Type = "CheckedBaggage",
                Quantity = baggage.CheckedCount,
                UnitPrice = bagPrice * bagMultiplier,
                Subtotal = checkedBaggageSubtotal
            });
        }

        decimal seatsSubtotal = detailByClass.Sum(d => d.Subtotal);
        decimal totalPaid = seatsSubtotal + handBaggageSubtotal + checkedBaggageSubtotal;

        return new PurchaseTotals
        {
            TotalPaid             = totalPaid,
            TotalSeats            = detailByClass.Sum(d => d.SeatCount),
            DetailByClass         = detailByClass,
            BaggageDetails        = baggageDetails,
            HandBaggageCount      = baggage.HandCount,
            HandBaggageSubtotal   = handBaggageSubtotal,
            CheckedBaggageCount   = baggage.CheckedCount,
            CheckedBaggageSubtotal = checkedBaggageSubtotal
        };
    }
}