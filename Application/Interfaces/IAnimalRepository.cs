public interface IAnimalRepository
{
    Task<List<Animal>> GetAllAsync();
    Task<Animal?> GetByIdAsync(int id);
    Task AddAsync(Animal animal);
    Task<bool> UpdateAsync(int id, Animal animal);
    Task<bool> DeleteAsync(int id);
}