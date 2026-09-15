using AnimalShelter.Application.Interfaces;
using AnimalShelter.Domain;
using AnimalShelter.Infrastructure.Data;

namespace AnimalShelter.Infrastructure.Repositories;

public class AdoptionRepository : IAdoptionRepository
{
    private readonly AppDbContext _db;

    public AdoptionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Adoption adoption)
    {
        await _db.Adoptions.AddAsync(adoption);
    }
}