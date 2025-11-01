namespace SpendWise.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string email);
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}
