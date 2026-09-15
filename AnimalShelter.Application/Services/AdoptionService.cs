using AnimalShelter.Application.DTOs;
using AnimalShelter.Application.Exceptions;
using AnimalShelter.Application.Interfaces;
using AnimalShelter.Domain;

namespace AnimalShelter.Application.Services;

public class AdoptionService : IAdoptionService
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IAdoptionRepository _adoptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdoptionService(
        IAnimalRepository animalRepository,
        IAdoptionRepository adoptionRepository,
        IUnitOfWork unitOfWork)
    {
        _animalRepository = animalRepository;
        _adoptionRepository = adoptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AdoptionDto> AdoptAsync(
    int animalId,
    AdoptAnimalDto dto)
    {
        Adoption? createdAdoption = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var animal =
                await _animalRepository.GetByIdAsync(animalId);

            if (animal == null)
            {
                throw new ResourceNotFoundException(
                    $"Animal with id {animalId} does not exist.");
            }

            if (animal.IsAdopted)
            {
                throw new ConflictException(
                    $"Animal with id {animalId} is already adopted.");
            }

            createdAdoption = new Adoption
            {
                AnimalId = animal.Id,
                AdopterName = dto.AdopterName,
                AdoptedAt = DateTime.UtcNow
            };

            await _adoptionRepository.AddAsync(
                createdAdoption);

            animal.IsAdopted = true;

            await _unitOfWork.SaveChangesAsync();
        });

        return new AdoptionDto
        {
            Id = createdAdoption!.Id,
            AnimalId = createdAdoption.AnimalId,
            AdopterName = createdAdoption.AdopterName,
            AdoptedAt = createdAdoption.AdoptedAt
        };
    }
}