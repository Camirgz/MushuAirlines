using backend.Model;

namespace backend.Repositories;

public interface IExternalApiRepository
{
    bool ValidateApiKey(string apiKeyHashed);
    void InsertConsumer(APIConsumerModel consumer);
    List<dynamic> GetFlights(string destination);
}