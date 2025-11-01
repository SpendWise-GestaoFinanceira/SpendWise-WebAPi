namespace SpendWise.Domain.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
    Task<bool> SendWelcomeEmailAsync(string email, string userName);
}
