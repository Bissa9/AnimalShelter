using AnimalShelter.Application.DTOs;
using AnimalShelter.Application.Exceptions;
using AnimalShelter.Application.Interfaces;
using AnimalShelter.Application.Services;
using AnimalShelter.Domain;
using Moq;

namespace AnimalShelter.Tests.Services;

public class AdoptionServiceTests
{
    [Fact]
    public async Task AdoptAsync_WhenAnimalIsAvailable_ShouldCreateAdoption()
    {
        // Arrange
        var animalRepository = new Mock<IAnimalRepository>();
        var adoptionRepository = new Mock<IAdoptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var animal = new Animal
        {
            Id = 4,
            Name = "Max",
            Age = 6,
            Type = "Dog",
            ShelterId = 2,
            IsAdopted = false
        };

        animalRepository
            .Setup(x => x.GetByIdAsync(4))
            .ReturnsAsync(animal);

        adoptionRepository
            .Setup(x => x.AddAsync(It.IsAny<Adoption>()))
            .Callback<Adoption>(adoption => adoption.Id = 1);

        unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> action) => action());

        var service = new AdoptionService(
            animalRepository.Object,
            adoptionRepository.Object,
            unitOfWork.Object);

        var dto = new AdoptAnimalDto
        {
            AdopterName = "Bissa"
        };

        // Act
        var result = await service.AdoptAsync(4, dto);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal(4, result.AnimalId);
        Assert.Equal("Bissa", result.AdopterName);

        Assert.True(animal.IsAdopted);

        adoptionRepository.Verify(
            x => x.AddAsync(It.IsAny<Adoption>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        unitOfWork.Verify(
            x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()),
            Times.Once);
    }

    [Fact]
    public async Task AdoptAsync_WhenAnimalIsAlreadyAdopted_ShouldThrowConflictException()
    {
        // Arrange
        var animalRepository = new Mock<IAnimalRepository>();
        var adoptionRepository = new Mock<IAdoptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var animal = new Animal
        {
            Id = 4,
            Name = "Max",
            Age = 6,
            Type = "Dog",
            ShelterId = 2,
            IsAdopted = true
        };

        animalRepository
            .Setup(x => x.GetByIdAsync(4))
            .ReturnsAsync(animal);

        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> action) => action());

        var service = new AdoptionService(
            animalRepository.Object,
            adoptionRepository.Object,
            unitOfWork.Object);

        var dto = new AdoptAnimalDto
        {
            AdopterName = "Another Person"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => service.AdoptAsync(4, dto));

        adoptionRepository.Verify(
            x => x.AddAsync(It.IsAny<Adoption>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task AdoptAsync_WhenAnimalDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        var animalRepository = new Mock<IAnimalRepository>();
        var adoptionRepository = new Mock<IAdoptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        animalRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Animal?)null);

        unitOfWork
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> action) => action());

        var service = new AdoptionService(
            animalRepository.Object,
            adoptionRepository.Object,
            unitOfWork.Object);

        var dto = new AdoptAnimalDto
        {
            AdopterName = "Bissa"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => service.AdoptAsync(999, dto));

        adoptionRepository.Verify(
            x => x.AddAsync(It.IsAny<Adoption>()),
            Times.Never);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
}
