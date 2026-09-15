using AnimalShelter.Domain;

namespace AnimalShelter.Application.Interfaces;

public interface IAdoptionRepository
{
    Task AddAsync(Adoption adoption);
}