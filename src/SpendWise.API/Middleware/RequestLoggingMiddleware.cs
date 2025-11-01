using Serilog.Context;
using System.Diagnostics;

namespace SpendWise.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Gerar correlation ID único para cada request
        var correlationId = Guid.NewGuid().ToString();
        
        // Adicionar correlation ID ao contexto de log
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        using (LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString()))
        {
            // Adicionar correlation ID no header da resposta
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            
            var stopwatch = Stopwatch.StartNew();
            
            _logger.LogInformation("HTTP {RequestMethod} {RequestPath} iniciado",
                context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
                
                stopwatch.Stop();
                
                _logger.LogInformation("HTTP {RequestMethod} {RequestPath} respondido {StatusCode} em {ElapsedMilliseconds}ms",
                    context.Request.Method, 
                    context.Request.Path, 
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex, "HTTP {RequestMethod} {RequestPath} falhou em {ElapsedMilliseconds}ms: {ExceptionMessage}",
                    context.Request.Method, 
                    context.Request.Path, 
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                
                throw; // Re-throw para que outros middlewares possam tratar
            }
        }
    }
}
