namespace AnimalShelter.Application.DTOs;

public class AnimalQueryDto
{
    public int? Page { get; set; }

    public int? PageSize { get; set; }

    public string? Type { get; set; }

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

}