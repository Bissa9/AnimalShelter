using AnimalShelter.Application.Interfaces;
using AnimalShelter.Domain;
using AnimalShelter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnimalShelter.Infrastructure.Repositories;

public class AnimalRepository : IAnimalRepository {

    private readonly AppDbContext _db;

    public AnimalRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Animal>> GetAllAsync()
    {
        return await _db.Animals
        .Include(a => a.Shelter)
        .ToListAsync();
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

    public async Task<(List<Animal> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    string? type,
    string? search,
    string? sortBy,
    string? sortDirection)
    {
        var query = _db.Animals
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(a => a.Type == type);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                a.Name.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var isDescending =
            string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "name" => isDescending
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name),

            "age" => isDescending
                ? query.OrderByDescending(a => a.Age)
                : query.OrderBy(a => a.Age),

            "type" => isDescending
                ? query.OrderByDescending(a => a.Type)
                : query.OrderBy(a => a.Type),

            _ => query.OrderBy(a => a.Id)
        };


        var items = await query
            .OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.Shelter)
            .ToListAsync();

        return (items, totalCount);
    }

    
}