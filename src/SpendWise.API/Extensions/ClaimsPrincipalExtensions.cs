using System.Security.Claims;

namespace SpendWise.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? user.FindFirst("sub")?.Value 
                         ?? user.FindFirst("id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido: ID do usuário não encontrado");
        }
        
        return userId;
    }
    
    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value 
               ?? user.FindFirst("email")?.Value
               ?? throw new UnauthorizedAccessException("Token inválido: Email do usuário não encontrado");
    }
}
