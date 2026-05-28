namespace backend.Interfaces;

public interface IItineraryRepository
{
    Task<int> CreateItineraryAsync(int passengerId);
}
