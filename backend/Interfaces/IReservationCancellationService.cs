using backend.Model;

namespace backend.Interfaces
{
    public interface IReservationCancellationService
    {
        void SendCancellationEmail(string reservationCode);

        void CancelReservation(string token);
    }
}