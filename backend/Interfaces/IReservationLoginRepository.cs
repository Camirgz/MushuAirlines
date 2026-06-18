using backend.Model;

namespace backend.Repositories
{
    public interface IReservationLoginRepository
    {
        bool ReservationExists(ReservationLoginModel model);
    }
}