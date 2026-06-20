using backend.Interfaces;
using backend.Model;

namespace backend.Services
{
    public class ReservationReportService : IReservationReportService
    {
        private readonly IPurchaseConfirmationRepository repository;

        public ReservationReportService(
            IPurchaseConfirmationRepository repository)
        {
            this.repository = repository;
        }

        public PurchaseConfirmationModel GetReservation(string reservationCode)
        {
            int purchaseId = repository.GetPurchaseIdByReservationCode(reservationCode);

            var reservation = repository.GetPurchase(purchaseId);

            if (reservation == null)
                throw new Exception("Reserva no encontrada.");

            return reservation;
        }

        public AddBaggageResponse AddBaggage(string reservationCode, AddBaggageRequest request)
        {
            var validAdditions = request.Passengers
                .Where(p => p.ExtraBags > 0)
                .ToList();

            if (validAdditions.Count == 0)
                throw new ArgumentException("Debe seleccionar al menos una maleta adicional.");

            int purchaseId = repository.GetPurchaseIdByReservationCode(reservationCode);

            var legs = repository.GetBagPricingByPurchaseId(purchaseId);
            decimal totalBagPrice = legs.Sum(l => l.BagPrice);

            var currentBags = repository.GetPassengerBaggageDetails(purchaseId)
                .ToDictionary(p => p.PassengerFullName, p => p.CheckedBagCount);

            var updates = validAdditions
                .Select(addition =>
                {
                    int current = currentBags.GetValueOrDefault(addition.PassengerFullName, 0);
                    decimal extraCost = legs.Sum(leg =>
                        BagSubtotal(current + addition.ExtraBags, leg.BagPrice, leg.BagMultiplier)
                        - BagSubtotal(current, leg.BagPrice, leg.BagMultiplier));

                    return new PassengerBaggageUpdate
                    {
                        PassengerFullName = addition.PassengerFullName,
                        ExtraBags         = addition.ExtraBags,
                        ExtraCost         = extraCost
                    };
                })
                .ToList();

            decimal totalCharged   = updates.Sum(u => u.ExtraCost);
            int     totalBagsAdded = updates.Sum(u => u.ExtraBags);

            repository.AddCheckedBagsToTickets(purchaseId, updates, totalBagPrice, totalCharged);

            return new AddBaggageResponse
            {
                TotalCharged   = totalCharged,
                TotalBagsAdded = totalBagsAdded
            };
        }

        private static decimal BagSubtotal(int count, decimal price, decimal multiplier)
            => count switch
            {
                0 => 0m,
                1 => price,
                _ => price + (count - 1) * price * multiplier
            };
    }
}