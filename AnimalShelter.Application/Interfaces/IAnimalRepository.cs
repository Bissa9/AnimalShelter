using AnimalShelter.Domain;
namespace AnimalShelter.Application.Interfaces;
public interface IAnimalRepository
{
    Task<List<Animal>> GetAllAsync();
    Task<Animal?> GetByIdAsync(int id);
    Task AddAsync(Animal animal);
    Task<bool> UpdateAsync(int id, Animal animal);
    Task<bool> DeleteAsync(int id);
    Task<(List<Animal> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    string? type,
    string? search,
    string? sortBy,
    string? sortDirection);
}