using System.ComponentModel.DataAnnotations;

namespace AnimalShelter.Application.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = "";
}