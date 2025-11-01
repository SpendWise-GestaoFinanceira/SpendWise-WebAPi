using Microsoft.Extensions.Logging;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
    {
        // Simular envio de email
        await Task.Delay(100); // Simular latência
        
        _logger.LogInformation("=== MOCK EMAIL - RESET DE SENHA ===");
        _logger.LogInformation("Para: {Email}", email);
        _logger.LogInformation("Token: {Token}", resetToken);
        _logger.LogInformation("URL: {Url}", resetUrl);
        _logger.LogInformation("====================================");
        
        return true;
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string userName)
    {
        // Simular envio de email
        await Task.Delay(100); // Simular latência
        
        _logger.LogInformation("=== MOCK EMAIL - BOAS-VINDAS ===");
        _logger.LogInformation("Para: {Email}", email);
        _logger.LogInformation("Nome: {UserName}", userName);
        _logger.LogInformation("================================");
        
        return true;
    }
}
