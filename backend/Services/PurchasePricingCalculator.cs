using backend.Interfaces;
using backend.Model;

namespace backend.Services;

public class PurchasePricingCalculator : IPricingCalculator
{
    public PurchaseTotals Calculate(
        IEnumerable<SeatSelection> seats,
        IEnumerable<PassengerInfo> passengers,
        decimal economyPrice,
        decimal firstClassPrice,
        decimal handBagPrice,
        decimal bagPrice,
        decimal bagMultiplier)
    {
        var seatList      = seats.ToList();
        var passengerList = passengers.ToList();

        var detailByClass = seatList
            .GroupBy(s => s.SeatClass)
            .Select(group =>
            {
                var unitPrice = group.Key switch
                {
                    SeatClass.Economy    => economyPrice,
                    SeatClass.FirstClass => firstClassPrice,
                    _ => throw new ArgumentException($"Clase de asiento desconocida: '{group.Key}'.")
                };

                var seatcount = group.Count();

                return new SeatClassSubtotal
                {
                    SeatClass = group.Key,
                    SeatCount = seatcount,
                    Subtotal  = seatcount * unitPrice
                };
            })
            .ToList();

        var passengerBaggageDetails = passengerList
            .Select((p, idx) => new PassengerBaggageSubtotal
            {
                PassengerIndex  = idx,
                HandBagCount    = p.HandBagCount,
                CheckedBagCount = p.CheckedBagCount,
                HandSubtotal    = BagSubtotal(p.HandBagCount,    handBagPrice, bagMultiplier),
                CheckedSubtotal = BagSubtotal(p.CheckedBagCount, bagPrice,     bagMultiplier)
            })
            .ToList();

        int     totalHandCount         = passengerBaggageDetails.Sum(p => p.HandBagCount);
        int     totalCheckedCount      = passengerBaggageDetails.Sum(p => p.CheckedBagCount);
        decimal handBaggageSubtotal    = passengerBaggageDetails.Sum(p => p.HandSubtotal);
        decimal checkedBaggageSubtotal = passengerBaggageDetails.Sum(p => p.CheckedSubtotal);

        var baggageDetails = new List<BaggageSubtotal>();
        if (totalHandCount > 0)
            baggageDetails.Add(new BaggageSubtotal
            {
                Type      = BaggageType.HandBaggage,
                Quantity  = totalHandCount,
                UnitPrice = handBagPrice,
                Subtotal  = handBaggageSubtotal
            });

        if (totalCheckedCount > 0)
            baggageDetails.Add(new BaggageSubtotal
            {
                Type      = BaggageType.CheckedBaggage,
                Quantity  = totalCheckedCount,
                UnitPrice = bagPrice,
                Subtotal  = checkedBaggageSubtotal
            });

        decimal seatsSubtotal = detailByClass.Sum(d => d.Subtotal);
        decimal totalPaid     = seatsSubtotal + handBaggageSubtotal + checkedBaggageSubtotal;

        return new PurchaseTotals
        {
            TotalPaid               = totalPaid,
            TotalSeats              = detailByClass.Sum(d => d.SeatCount),
            DetailByClass           = detailByClass,
            BaggageDetails          = baggageDetails,
            PassengerBaggageDetails = passengerBaggageDetails,
            HandBaggageCount        = totalHandCount,
            HandBaggageSubtotal     = handBaggageSubtotal,
            CheckedBaggageCount     = totalCheckedCount,
            CheckedBaggageSubtotal  = checkedBaggageSubtotal
        };
    }

    private static decimal BagSubtotal(int count, decimal unitPrice, decimal multiplier)
        => count switch
        {
            0 => 0m,
            1 => unitPrice,
            _ => unitPrice + (count - 1) * unitPrice * multiplier
        };
}