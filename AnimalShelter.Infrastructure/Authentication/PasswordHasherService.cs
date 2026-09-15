using AnimalShelter.Application.Interfaces;
using AnimalShelter.Domain;
using Microsoft.AspNetCore.Identity;

namespace AnimalShelter.Infrastructure.Authentication;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordHasherService(
        IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(
        User user,
        string hashedPassword,
        string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            hashedPassword,
            password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}