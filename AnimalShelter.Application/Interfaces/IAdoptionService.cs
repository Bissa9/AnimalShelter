using AnimalShelter.Application.DTOs;

namespace AnimalShelter.Application.Interfaces;

public interface IAdoptionService
{
    Task<AdoptionDto> AdoptAsync(
        int animalId,
        AdoptAnimalDto dto);
}