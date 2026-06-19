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
            int purchaseId =
                repository.GetPurchaseIdByReservationCode(reservationCode);

            var reservation =
                repository.GetPurchase(purchaseId);

            if (reservation == null)
            {
                throw new Exception("Reserva no encontrada.");
            }

            return reservation;
        }
    }
}