namespace AnimalShelter.Application.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = "";

    public string RefreshToken { get; set; } = "";
}