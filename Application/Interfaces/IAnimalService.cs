public interface IAnimalService
{
    Task<List<Animal>> GetAllAsync();

    Task<AnimalDto?> GetByIdAsync(int id);

    Task<Animal> CreateAsync(Animal animal);

    Task<bool> UpdateAsync(int id, Animal animal);

    Task<bool> DeleteAsync(int id);
}