using AnimalShelter.Application.DTOs;
using AnimalShelter.Application.Interfaces;
using AnimalShelter.Application.Services;
using AnimalShelter.Application.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using AnimalShelter.Domain;
using Moq;

namespace AnimalShelter.Tests.Services;

public class AnimalServiceTests
{
    private readonly IDistributedCache cache =
    new MemoryDistributedCache(
        Options.Create(new MemoryDistributedCacheOptions()));

    [Fact]
    public async Task CreateAsync_WhenShelterExists_ShouldCreateAnimal()
    {
        // Arrange
        var animalRepository = new Mock<IAnimalRepository>();
        var shelterRepository = new Mock<IShelterRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var createdAnimal = new Animal
        {
            Id = 1,
            Name = "Milo",
            Age = 3,
            Type = "Dog",
            ShelterId = 2,
            IsAdopted = false,
            Shelter = new Shelter
            {
                Id = 2,
                Name = "Happy Paws",
                Location = "Alexandria"
            }
        };

        shelterRepository
            .Setup(x => x.ExistsAsync(2))
            .ReturnsAsync(true);

        animalRepository
            .Setup(x => x.AddAsync(It.IsAny<Animal>()))
            .Callback<Animal>(animal => animal.Id = 1);

        animalRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(createdAnimal);

        unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new AnimalService(
            animalRepository.Object,
            unitOfWork.Object,
            shelterRepository.Object,
            NullLogger<AnimalService>.Instance,
            cache);

        var dto = new CreateAnimalDto
        {
            Name = "Milo",
            Age = 3,
            Type = "Dog",
            ShelterId = 2
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Milo", result.Name);
        Assert.Equal("Dog", result.Type);
        Assert.Equal(2, result.ShelterId);
        Assert.Equal("Happy Paws", result.ShelterName);

        shelterRepository.Verify(
            x => x.ExistsAsync(2),
            Times.Once);

        animalRepository.Verify(
            x => x.AddAsync(It.IsAny<Animal>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenShelterDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        var animalRepository = new Mock<IAnimalRepository>();
        var shelterRepository = new Mock<IShelterRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        shelterRepository
            .Setup(x => x.ExistsAsync(999))
            .ReturnsAsync(false);

        var service = new AnimalService(
            animalRepository.Object,
            unitOfWork.Object,
            shelterRepository.Object,
            NullLogger<AnimalService>.Instance,
            cache);

        var dto = new CreateAnimalDto
        {
            Name = "Milo",
            Age = 3,
            Type = "Dog",
            ShelterId = 999
        };

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => service.CreateAsync(dto));

        animalRepository.Verify(
            x => x.AddAsync(It.IsAny<Animal>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
}