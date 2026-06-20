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

            var pricing = repository.GetBagPricingByPurchaseId(purchaseId);

            var currentBags = repository.GetPassengerBaggageDetails(purchaseId)
                .ToDictionary(p => p.PassengerFullName, p => p.CheckedBagCount);

            var updates = validAdditions
                .Select(addition =>
                {
                    int current = currentBags.GetValueOrDefault(addition.PassengerFullName, 0);
                    decimal extraCost =
                        BagSubtotal(current + addition.ExtraBags, pricing.BagPrice, pricing.BagMultiplier)
                        - BagSubtotal(current, pricing.BagPrice, pricing.BagMultiplier);

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

            repository.AddCheckedBagsToTickets(purchaseId, updates, pricing.BagPrice, totalCharged);

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