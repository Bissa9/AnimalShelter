using AnimalShelter.Domain;

namespace AnimalShelter.Application.Interfaces;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(
        User user,
        string hashedPassword,
        string password);
}