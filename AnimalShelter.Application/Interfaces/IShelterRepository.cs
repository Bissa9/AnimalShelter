namespace AnimalShelter.Application.Interfaces;

public interface IShelterRepository
{
    Task<bool> ExistsAsync(int id);
}