using backend.Model;

namespace backend.Interfaces
{
    public interface IReservationReportService
    {
        PurchaseConfirmationModel GetReservation(string reservationCode);
    }
}