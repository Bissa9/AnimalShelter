using AnimalShelter.Application.DTOs;
namespace AnimalShelter.Application.Interfaces;

public interface IAnimalService
{
    Task<List<AnimalDto>> GetAllAsync();
    Task<AnimalDto> GetByIdAsync(int id);
    Task<AnimalDto> CreateAsync(CreateAnimalDto dto);
    Task<bool> UpdateAsync(int id, UpdateAnimalDto dto);
    Task DeleteAsync(int id);
    Task<PagedResultDto<AnimalDto>> GetPagedAsync(
    AnimalQueryDto query);
}