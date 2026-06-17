using backend.Model;

namespace backend.Services
{
    public interface IReservationLoginService
    {
        string ReservationLogin(ReservationLoginModel model);
    }
}