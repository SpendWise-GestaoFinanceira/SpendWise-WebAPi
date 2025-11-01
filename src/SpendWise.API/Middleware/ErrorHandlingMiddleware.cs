using System.Net;
using System.Text.Json;
using FluentValidation;
using SpendWise.Domain.Exceptions;

namespace SpendWise.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada capturada: {ExceptionType} - {Message} | RequestMethod: {RequestMethod} | RequestPath: {RequestPath}",
                ex.GetType().Name, ex.Message, context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response;
        int statusCode;

        switch (exception)
        {
            case MesFechadoException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    erro = new
                    {
                        mensagem = "Operação não permitida",
                        detalhes = exception.Message,
                        codigo = "MES_FECHADO"
                    }
                };
                break;
                
            case ValidationException validationEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    erro = new
                    {
                        mensagem = "Falha na validação dos dados",
                        detalhes = validationEx.Errors.Select(e => new
                        {
                            campo = e.PropertyName,
                            mensagemErro = e.ErrorMessage,
                            valorTentativa = e.AttemptedValue
                        })
                    }
                };
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    erro = new
                    {
                        mensagem = "Recurso não encontrado",
                        detalhes = exception.Message
                    }
                };
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                response = new
                {
                    erro = new
                    {
                        mensagem = "Acesso negado",
                        detalhes = exception.Message
                    }
                };
                break;

            case ArgumentException:
            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    erro = new
                    {
                        mensagem = "Requisição inválida",
                        detalhes = exception.Message
                    }
                };
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    erro = new
                    {
                        mensagem = "Ocorreu um erro interno no servidor",
                        detalhes = "Entre em contato com o suporte se o problema persistir"
                    }
                };
                break;
        }

        context.Response.StatusCode = statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}