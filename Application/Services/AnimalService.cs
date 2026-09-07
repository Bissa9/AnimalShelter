public class AnimalService : IAnimalService
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AnimalService(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Animal>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<AnimalDto?> GetByIdAsync(int id)
    {
        var animal = await _repository.GetByIdAsync(id);

        if (animal == null)
            return null;

        return new AnimalDto
        {
            Id = animal.Id,
            Name = animal.Name,
            Age = animal.Age,
            Type = animal.Type,
            ShelterId = animal.ShelterId,
            ShelterName = animal.Shelter?.Name ?? "",
            ShelterLocation = animal.Shelter?.Location ?? ""
        };
    }

    public async Task<Animal> CreateAsync(Animal animal)
    {
        await _repository.AddAsync(animal);

        await _unitOfWork.SaveChangesAsync();

        return animal;
    }

    public async Task<bool> UpdateAsync(int id, Animal animal)
    {
        var updated = await _repository.UpdateAsync(id, animal);

        if (!updated)
            return false;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return false;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}