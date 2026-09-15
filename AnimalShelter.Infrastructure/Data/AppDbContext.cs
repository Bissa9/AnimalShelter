using AnimalShelter.Domain;
using Microsoft.EntityFrameworkCore;
namespace AnimalShelter.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    

    public DbSet<Animal> Animals { get; set; }
    public DbSet<Shelter> Shelters { get; set; }
    public DbSet<Adoption> Adoptions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>()
            .ToTable("animals");

        modelBuilder.Entity<Shelter>()
            .ToTable("shelter");

        modelBuilder.Entity<Animal>()
            .HasOne(a => a.Shelter)
            .WithMany(s => s.Animals)
            .HasForeignKey(a => a.ShelterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Adoption>()
            .HasOne(a => a.Animal)
            .WithOne()
            .HasForeignKey<Adoption>(a => a.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .ToTable("users");

        modelBuilder.Entity<RefreshToken>()
            .ToTable("refresh_tokens");

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Animal>()
            .Property(a => a.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<Animal>()
            .HasIndex(a => a.Type);

        modelBuilder.Entity<Animal>()
            .HasIndex(a => a.Name);

        modelBuilder.Entity<Animal>()
            .HasIndex(a => new { a.Type, a.Name });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Animal>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version = Guid.NewGuid();
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}