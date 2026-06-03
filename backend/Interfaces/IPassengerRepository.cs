using backend.Model;

namespace backend.Interfaces;

public interface IPassengerRepository
{
    Task<int?> FindPassengerByDocumentAsync(string passportNumber);
    Task<int> CreatePassengerAsync(PassengerInfo data);
}
