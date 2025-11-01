using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendWise.Infrastructure.Data;
using System.Diagnostics;

namespace SpendWise.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint para verificar se a API está funcionando
    /// </summary>
    /// <returns>Status de saúde da aplicação</returns>
    [HttpGet]
    public async Task<ActionResult<object>> GetHealth()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            // Verificar conectividade com o banco de dados
            var canConnectToDb = await CheckDatabaseConnectionAsync();
            
            var responseTime = DateTime.UtcNow - startTime;
            
            var healthStatus = new
            {
                status = canConnectToDb ? "healthy" : "unhealthy",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                version = GetType().Assembly.GetName().Version?.ToString() ?? "unknown",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
                checks = new
                {
                    database = new
                    {
                        status = canConnectToDb ? "healthy" : "unhealthy",
                        responseTime = $"{responseTime.TotalMilliseconds:F2}ms"
                    },
                    liveness = new
                    {
                        status = "healthy"
                    }
                },
                responseTimeMs = responseTime.TotalMilliseconds
            };

            _logger.LogInformation("Health check completed. Status: {Status}, Database: {DatabaseStatus}, ResponseTime: {ResponseTime}ms",
                healthStatus.status, canConnectToDb ? "healthy" : "unhealthy", responseTime.TotalMilliseconds);

            return canConnectToDb ? Ok(healthStatus) : StatusCode(503, healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            
            var errorResponse = new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                error = "Health check failed",
                details = ex.Message
            };

            return StatusCode(503, errorResponse);
        }
    }

    /// <summary>
    /// Readiness check - verifica se a aplicação está pronta para receber tráfego
    /// </summary>
    [HttpGet("ready")]
    public async Task<ActionResult<object>> GetReadiness()
    {
        try
        {
            // Verificar se o banco está acessível e migrado
            var canConnectToDb = await CheckDatabaseConnectionAsync();
            var hasPendingMigrations = await CheckPendingMigrationsAsync();
            
            var isReady = canConnectToDb && !hasPendingMigrations;
            
            var readinessStatus = new
            {
                status = isReady ? "ready" : "not ready",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                checks = new
                {
                    database = canConnectToDb ? "ready" : "not ready",
                    migrations = !hasPendingMigrations ? "ready" : "pending migrations"
                }
            };

            return isReady ? Ok(readinessStatus) : StatusCode(503, readinessStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed with exception");
            
            var errorResponse = new
            {
                status = "not ready",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                error = "Readiness check failed",
                details = ex.Message
            };

            return StatusCode(503, errorResponse);
        }
    }

    /// <summary>
    /// Liveness check - verifica se a aplicação está viva
    /// </summary>
    [HttpGet("live")]
    public ActionResult<object> GetLiveness()
    {
        var livenessStatus = new
        {
            status = "alive",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
        };

        return Ok(livenessStatus);
    }

    private async Task<bool> CheckDatabaseConnectionAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckPendingMigrationsAsync()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            return pendingMigrations.Any();
        }
        catch
        {
            return true; // Se houver erro, assumir que há migrações pendentes
        }
    }
}
