using AnimalShelter.Application.Interfaces;
using AnimalShelter.Domain;
using AnimalShelter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnimalShelter.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
    }
}