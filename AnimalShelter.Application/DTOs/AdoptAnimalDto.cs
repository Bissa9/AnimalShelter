using System.ComponentModel.DataAnnotations;

namespace AnimalShelter.Application.DTOs;

public class AdoptAnimalDto
{
    [Required]
    public string AdopterName { get; set; } = "";
}