namespace AnimalShelter.Application.DTOs;
using System.ComponentModel.DataAnnotations;
public class CreateAnimalDto 
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name{ get; set;} = "";
    [Range(0, 100)]
    public int Age {get; set;}
    [Required]
    [StringLength(50)]
    public string Type {get; set;} = "";
    [Range(1, int.MaxValue)]
    public int ShelterId { get; set; }
}