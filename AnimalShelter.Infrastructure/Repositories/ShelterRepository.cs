using AnimalShelter.Application.Interfaces;
using AnimalShelter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnimalShelter.Infrastructure.Repositories;

public class ShelterRepository : IShelterRepository
{
    private readonly AppDbContext _db;

    public ShelterRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _db.Shelters.AnyAsync(s => s.Id == id);
    }
}