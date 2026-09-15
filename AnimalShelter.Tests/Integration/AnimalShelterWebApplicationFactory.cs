using AnimalShelter.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AnimalShelter.Domain;
using Microsoft.AspNetCore.Identity;

namespace AnimalShelter.Tests.Integration;

public class AnimalShelterWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly SemaphoreSlim DatabaseLock = new(1, 1);
    private static bool _databaseInitialized;

    protected override void ConfigureWebHost(
    IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection")
            ?? "Server=127.0.0.1;Port=3307;Database=AnimalShelterDb;User=root;Password=rootpassword;";

        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            connectionString);

        var redisConnection =
            Environment.GetEnvironmentVariable(
                "Redis__Connection")
            ?? "127.0.0.1:6379";

        builder.UseSetting(
            "Redis:Connection",
            redisConnection);

        builder.UseSetting(
        "Jwt:Key",
        "ThisIsATestOnlyJwtKey12345678901234567890");

        builder.UseSetting(
            "Jwt:Issuer",
            "AnimalShelter.API");

        builder.UseSetting(
            "Jwt:Audience",
            "AnimalShelter.Client");
    }

    public async Task InitializeAsync()
    {
        if (_databaseInitialized)
        {
            return;
        }

        await DatabaseLock.WaitAsync();

        try
        {
            if (_databaseInitialized)
            {
                return;
            }

            using var scope = Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            await db.Database.MigrateAsync();

            var passwordHasher =
                scope.ServiceProvider
                    .GetRequiredService<IPasswordHasher<User>>();

            await DbSeeder.SeedAsync(
                db,
                passwordHasher);

            _databaseInitialized = true;
        }
        finally
        {
            DatabaseLock.Release();
        }
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}