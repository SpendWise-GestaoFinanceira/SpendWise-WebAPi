using MediatR;
using SpendWise.Application.DTOs.Transacoes;

namespace SpendWise.Application.Queries.Transacoes;

public record ExportTransacoesQuery(
    Guid UsuarioId,
    ExportTransacoesRequestDto Request
) : IRequest<ExportTransacoesResponseDto>;
