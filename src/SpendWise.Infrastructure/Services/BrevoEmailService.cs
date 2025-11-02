using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Infrastructure.Services;

public class BrevoEmailService : IEmailService
{
    private readonly ILogger<BrevoEmailService> _logger;
    private readonly TransactionalEmailsApi _apiInstance;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public BrevoEmailService(
        IConfiguration configuration,
        ILogger<BrevoEmailService> logger)
    {
        _logger = logger;

        var apiKey = configuration["EmailSettings:ApiKey"];
        _senderEmail = configuration["EmailSettings:SenderEmail"] ?? "noreply@spendwise.com";
        _senderName = configuration["EmailSettings:SenderName"] ?? "SpendWise";

        _logger.LogInformation("🔑 Brevo API Key configurada: {HasKey}", !string.IsNullOrEmpty(apiKey));
        _logger.LogInformation("📧 Sender Email: {Email}", _senderEmail);

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("❌ Brevo API Key NÃO configurada!");
            throw new InvalidOperationException("Brevo API Key não configurada");
        }

        Configuration.Default.ApiKey.Add("api-key", apiKey);
        _apiInstance = new TransactionalEmailsApi();
        _logger.LogInformation("✅ BrevoEmailService inicializado com sucesso");
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
    {
        try
        {
            var sendSmtpEmail = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(_senderName, _senderEmail),
                To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email) },
                Subject = "🔐 Redefinir Senha - SpendWise",
                HtmlContent = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); 
                                      color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                            .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                            .button {{ display: inline-block; padding: 15px 30px; background: #10b981; 
                                      color: white; text-decoration: none; border-radius: 8px; 
                                      font-weight: bold; margin: 20px 0; }}
                            .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🔐 Redefinir Senha</h1>
                            </div>
                            <div class='content'>
                                <p>Olá,</p>
                                <p>Você solicitou a redefinição de senha da sua conta no <strong>SpendWise</strong>.</p>
                                <p>Clique no botão abaixo para criar uma nova senha:</p>
                                <div style='text-align: center;'>
                                    <a href='{resetUrl}' class='button'>Redefinir Senha</a>
                                </div>
                                <p><strong>⏰ Este link expira em 30 minutos.</strong></p>
                                <p>Se você não solicitou esta redefinição, ignore este email.</p>
                                <hr>
                                <p style='font-size: 12px; color: #666;'>
                                    Ou copie e cole este link no navegador:<br>
                                    <a href='{resetUrl}'>{resetUrl}</a>
                                </p>
                            </div>
                            <div class='footer'>
                                <p>© 2025 SpendWise - Gestão Financeira Inteligente</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };

            var result = await _apiInstance.SendTransacEmailAsync(sendSmtpEmail);
            _logger.LogInformation("✅ Email de reset enviado com sucesso para {Email}. MessageId: {MessageId}",
                email, result.MessageId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar email de reset para {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var sendSmtpEmail = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(_senderName, _senderEmail),
                To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email) },
                Subject = "🎉 Bem-vindo ao SpendWise!",
                HtmlContent = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); 
                                      color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                            .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                            .feature {{ background: white; padding: 15px; margin: 10px 0; 
                                       border-left: 4px solid #10b981; border-radius: 5px; }}
                            .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎉 Bem-vindo ao SpendWise!</h1>
                            </div>
                            <div class='content'>
                                <p>Olá <strong>{userName}</strong>,</p>
                                <p>Sua conta foi criada com sucesso! Estamos felizes em tê-lo conosco.</p>
                                
                                <h3>🚀 Comece agora:</h3>
                                <div class='feature'>
                                    <strong>📊 Dashboard</strong><br>
                                    Visualize suas finanças em tempo real
                                </div>
                                <div class='feature'>
                                    <strong>💰 Transações</strong><br>
                                    Registre receitas e despesas facilmente
                                </div>
                                <div class='feature'>
                                    <strong>🎯 Orçamentos</strong><br>
                                    Defina limites e controle seus gastos
                                </div>
                                <div class='feature'>
                                    <strong>📈 Relatórios</strong><br>
                                    Análises detalhadas das suas finanças
                                </div>
                                
                                <p style='margin-top: 20px;'>
                                    Acesse agora: <a href='http://localhost:3000/login'>SpendWise</a>
                                </p>
                            </div>
                            <div class='footer'>
                                <p>© 2025 SpendWise - Gestão Financeira Inteligente</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };

            var result = await _apiInstance.SendTransacEmailAsync(sendSmtpEmail);
            _logger.LogInformation("✅ Email de boas-vindas enviado para {Email}. MessageId: {MessageId}",
                email, result.MessageId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar email de boas-vindas para {Email}", email);
            return false;
        }
    }
}
