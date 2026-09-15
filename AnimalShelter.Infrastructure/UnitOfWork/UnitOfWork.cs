using AnimalShelter.Application.Interfaces;
using AnimalShelter.Infrastructure.Data;

namespace AnimalShelter.Infrastructure.UnitOfWork;
public class UnitOfWork : IUnitOfWork {
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            await action();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}