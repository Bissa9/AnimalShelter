using AnimalShelter.Application.DTOs;

namespace AnimalShelter.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);

    Task<LoginResponseDto?> LoginAsync(LoginDto dto);

    Task<LoginResponseDto?> RefreshAsync(
        RefreshTokenRequestDto dto);
}