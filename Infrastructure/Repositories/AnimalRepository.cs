using Microsoft.EntityFrameworkCore;

public class AnimalRepository : IAnimalRepository {

    private readonly AppDbContext _db;

    public AnimalRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Animal>> GetAllAsync()
    {
        return await _db.Animals.ToListAsync();
    }   

    public async Task<Animal?> GetByIdAsync(int id)
    {
        return await _db.Animals
            .Include(a => a.Shelter)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Animal animal)
    {
        await _db.Animals.AddAsync(animal);
    }

    public async Task<bool> UpdateAsync(int id, Animal animal)
    {
        var existingAnimal = await _db.Animals
            .FirstOrDefaultAsync(a => a.Id == id);

        if (existingAnimal == null)
            return false;

        existingAnimal.Name = animal.Name;
        existingAnimal.Age = animal.Age;
        existingAnimal.Type = animal.Type;
    
        return true;    
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var animal = await _db.Animals
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if(animal == null)
            return false;

        _db.Animals.Remove(animal);
        return true;
    }

    
}