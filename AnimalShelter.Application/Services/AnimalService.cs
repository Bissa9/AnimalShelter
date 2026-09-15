using AnimalShelter.Application.DTOs;
using AnimalShelter.Application.Interfaces;
using AnimalShelter.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using AnimalShelter.Domain;

namespace AnimalShelter.Application.Services;

public class AnimalService : IAnimalService
{
    private readonly IAnimalRepository _repository;
    private readonly IShelterRepository _shelterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AnimalService> _logger;
    private readonly IDistributedCache _cache;

    public AnimalService(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork,
        IShelterRepository shelterRepository,
        ILogger<AnimalService> logger,
        IDistributedCache cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _shelterRepository = shelterRepository;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<AnimalDto>> GetAllAsync()
    {
        var animals = await _repository.GetAllAsync();

        return animals
            .Select(MapToDto)
            .ToList();
    }

    public async Task<AnimalDto> GetByIdAsync(int id)
    {
        var cacheKey = $"animal:{id}";

        var cachedJson = await _cache.GetStringAsync(cacheKey);

        if (cachedJson != null)
        {
            _logger.LogInformation(
                "Animal {AnimalId} returned from distributed cache.",
                id);

            return JsonSerializer.Deserialize<AnimalDto>(
                cachedJson)!;
        }

        var animal = await _repository.GetByIdAsync(id);

        if (animal == null)
        {
            throw new ResourceNotFoundException(
                $"Animal with id {id} was not found.");
        }

        var dto = MapToDto(animal);

        var json = JsonSerializer.Serialize(dto);

        await _cache.SetStringAsync(
            cacheKey,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(5)
            });

        _logger.LogInformation(
            "Animal {AnimalId} loaded from database and cached in Redis.",
            id);

        return dto;
    }

   public async Task<AnimalDto> CreateAsync(
    CreateAnimalDto dto)
    {
        _logger.LogInformation(
            "Creating animal {AnimalName} for shelter {ShelterId}.",
            dto.Name,
            dto.ShelterId);

        if (!await _shelterRepository.ExistsAsync(dto.ShelterId))
        {
            _logger.LogWarning(
                "Attempted to create animal {AnimalName} for non-existing shelter {ShelterId}.",
                dto.Name,
                dto.ShelterId);

            throw new ResourceNotFoundException(
                $"Shelter with id {dto.ShelterId} does not exist.");
        }

        var animal = new Animal
        {
            Name = dto.Name,
            Age = dto.Age,
            Type = dto.Type,
            ShelterId = dto.ShelterId
        };

        await _repository.AddAsync(animal);
        await _unitOfWork.SaveChangesAsync();

        var createdAnimal =
            await _repository.GetByIdAsync(animal.Id);

        if (createdAnimal == null)
        {
            _logger.LogError(
                "Animal {AnimalId} was created but could not be retrieved afterwards.",
                animal.Id);

            throw new InvalidOperationException(
                $"Animal with id {animal.Id} was created but could not be retrieved.");
        }

        _logger.LogInformation(
            "Animal {AnimalId} created successfully.",
            animal.Id);

        return MapToDto(createdAnimal);
    }

    public async Task<bool> UpdateAsync(int id, UpdateAnimalDto dto)
    {
        var animal = new Animal
        {
            Name = dto.Name,
            Age = dto.Age,
            Type = dto.Type
        };

        var updated = await _repository.UpdateAsync(id, animal);

        if (!updated)
            return false;

        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"animal:{id}");

        return true;
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            throw new ResourceNotFoundException(
                $"Animal with id {id} was not found.");
        }

        await _unitOfWork.SaveChangesAsync();

        await _cache.RemoveAsync($"animal:{id}");
    }

    public async Task<PagedResultDto<AnimalDto>> GetPagedAsync(
        AnimalQueryDto query)
    {
        var page = query.Page ?? 1;
        var pageSize = query.PageSize ?? 10;

        var (animals, totalCount) =
            await _repository.GetPagedAsync(
                page,
                pageSize,
                query.Type,
                query.Search,
                query.SortBy,
                query.SortDirection);

        var items = animals
            .Select(MapToDto)
            .ToList();

        var totalPages =
            (int)Math.Ceiling(
                (double)totalCount / pageSize);

        return new PagedResultDto<AnimalDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private static AnimalDto MapToDto(Animal animal)
    {
        return new AnimalDto
        {
            Id = animal.Id,
            Name = animal.Name,
            Age = animal.Age,
            Type = animal.Type,
            ShelterId = animal.ShelterId,
            ShelterName = animal.Shelter?.Name ?? "",
            ShelterLocation = animal.Shelter?.Location ?? ""
        };
    }
}