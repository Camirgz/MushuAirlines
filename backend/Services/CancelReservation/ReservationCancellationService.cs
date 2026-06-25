using backend.Interfaces;

namespace backend.Services
{
    public class ReservationCancellationService
        : IReservationCancellationService
    {
        private readonly IReservationCancellationRepository repository;
        private readonly IEmailCancellationService emailService;

        public ReservationCancellationService(
            IReservationCancellationRepository repository,
            IEmailCancellationService emailService)
        {
            this.repository = repository;
            this.emailService = emailService;
        }

        public void SendCancellationEmail(string reservationCode)
        {
            var reservation = repository.GetReservation(reservationCode);

            if (reservation == null)
                throw new Exception("Reserva no encontrada.");

            emailService.SendCancellationEmail(reservation);
        }

        public void CancelReservation(string token)
        {
            repository.CancelReservation(token);
        }
    }
}