using DogGroomingAPI.Models;

namespace DogGroomingAPI.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}
