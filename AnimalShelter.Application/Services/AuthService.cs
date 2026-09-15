using AnimalShelter.Application.DTOs;
using AnimalShelter.Application.Exceptions;
using AnimalShelter.Application.Interfaces;
using Microsoft.Extensions.Logging;
using AnimalShelter.Domain;

namespace AnimalShelter.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser =
            await _userRepository.GetByUsernameAsync(dto.Username);

        if (existingUser != null)
        {
            throw new ConflictException(
                $"Username '{dto.Username}' is already taken.");
        }

        var user = new User
        {
            Username = dto.Username,
            Role = "user"
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, dto.Password);

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user =
            await _userRepository.GetByUsernameAsync(dto.Username);

        if (user == null)
        {
            _logger.LogWarning(
                "Login attempt failed for username {Username}. User not found.",
                dto.Username);

            return null;
        }

        var validPassword =
            _passwordHasher.VerifyPassword(
                user,
                user.PasswordHash,
                dto.Password);

        if (!validPassword)
        {
            _logger.LogWarning(
                "Login attempt failed for username {Username}. Invalid password.",
                dto.Username);

            return null;
        }

        var accessToken = _tokenService.CreateToken(
            user.Username,
            user.Role);

        var refreshToken =
            _refreshTokenService.GenerateToken();

        var refreshTokenHash =
            _refreshTokenService.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserId = user.Id
        };

        await _refreshTokenRepository.AddAsync(
            refreshTokenEntity);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "User {Username} logged in successfully.",
            user.Username);

        return new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken
        };
    }
    public async Task<LoginResponseDto?> RefreshAsync(
    RefreshTokenRequestDto dto)
    {
        var tokenHash =
            _refreshTokenService.HashToken(dto.RefreshToken);

        var storedToken =
            await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (storedToken == null)
            return null;

        if (storedToken.IsRevoked)
            return null;

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
            return null;

        if (storedToken.User == null)
            return null;

        // Revoke the old refresh token
        storedToken.IsRevoked = true;

        // Create a new access token
        var accessToken =
            _tokenService.CreateToken(
                storedToken.User.Username,
                storedToken.User.Role);

        // Create a new refresh token
        var newRefreshToken =
            _refreshTokenService.GenerateToken();

        var newRefreshTokenHash =
            _refreshTokenService.HashToken(newRefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserId = storedToken.User.Id
        };

        await _refreshTokenRepository.AddAsync(
            newRefreshTokenEntity);

        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = newRefreshToken
        };
    }
}