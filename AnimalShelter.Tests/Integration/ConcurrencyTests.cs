using AnimalShelter.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AnimalShelter.Tests.Integration;

public class ConcurrencyTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConcurrencyTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdatingSameAnimalFromTwoContexts_ShouldThrowConcurrencyException()
    {
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var db1 = scope1.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var db2 = scope2.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var animal1 = await db1.Animals
            .FirstOrDefaultAsync();

        var animal2 = await db2.Animals
            .FirstOrDefaultAsync();

        Assert.NotNull(animal1);
        Assert.NotNull(animal2);

        Assert.Equal(animal1!.Id, animal2!.Id);
        Assert.Equal(animal1.Version, animal2.Version);

        var originalAge = animal1.Age;

        // Context 1 wins.
        animal1.Age = originalAge + 1;

        await db1.SaveChangesAsync();

        // Context 2 still has the old Version.
        animal2.Age = originalAge + 2;

        // Context 2 should now fail.
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () =>
            {
                await db2.SaveChangesAsync();
            });
    }
}