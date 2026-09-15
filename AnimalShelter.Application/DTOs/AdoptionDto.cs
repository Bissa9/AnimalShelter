namespace AnimalShelter.Application.DTOs;

public class AdoptionDto
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public string AdopterName { get; set; } = "";

    public DateTime AdoptedAt { get; set; }
}