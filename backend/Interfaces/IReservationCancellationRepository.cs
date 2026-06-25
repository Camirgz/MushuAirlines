using backend.Model;

namespace backend.Interfaces
{
    public interface IReservationCancellationRepository
    {
        CancellationReservationModel? GetReservation(string reservationCode);

        void CancelReservation(string token);
    }
}