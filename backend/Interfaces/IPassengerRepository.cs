using backend.Model;

namespace backend.Interfaces;

public interface IPassengerRepository
{
    Task<int?> FindPassengerByDocumentAsync(string documentType, string documentNumber);
    Task<int> CreatePassengerAsync(PassengerInfo data);
}
