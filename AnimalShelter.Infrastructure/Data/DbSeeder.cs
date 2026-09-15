using AnimalShelter.Domain;
using Microsoft.EntityFrameworkCore;

namespace AnimalShelter.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Create shelters only if none exist
        if (!await db.Shelters.AnyAsync())
        {
            var shelters = new List<Shelter>
            {
                new Shelter
                {
                    Id = 1,
                    Name = "Happy Paws",
                    Location = "Alexandria"
                },
                new Shelter
                {
                    Id = 2,
                    Name = "Safe Haven",
                    Location = "Cairo"
                }
            };

            await db.Shelters.AddRangeAsync(shelters);
            await db.SaveChangesAsync();
        }

        // Create animals only if none exist
        if (!await db.Animals.AnyAsync())
        {
            var animals = new List<Animal>
            {
                new Animal
                {
                    Id = 1,
                    Name = "Buddy",
                    Age = 3,
                    Type = "Dog",
                    ShelterId = 1,
                    IsAdopted = false
                },
                new Animal
                {
                    Id = 2,
                    Name = "Luna",
                    Age = 2,
                    Type = "Cat",
                    ShelterId = 1,
                    IsAdopted = false
                },
                new Animal
                {
                    Id = 3,
                    Name = "Rocky",
                    Age = 5,
                    Type = "Dog",
                    ShelterId = 2,
                    IsAdopted = false
                },
                new Animal
                {
                    Id = 4,
                    Name = "Max",
                    Age = 6,
                    Type = "Dog",
                    ShelterId = 2,
                    IsAdopted = false
                }
            };

            await db.Animals.AddRangeAsync(animals);
            await db.SaveChangesAsync();
        }
    }
}
