using backend.Model;

namespace backend.Interfaces;

public interface IPassengerRepository
{
    Task<int> CreatePassengerAsync(PassengerInfo data);
}
