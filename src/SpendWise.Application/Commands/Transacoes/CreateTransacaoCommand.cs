using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Commands.Transacoes;

public record CreateTransacaoCommand(
    string Descricao,
    Money Valor,
    DateTime DataTransacao,
    TipoTransacao Tipo,
    Guid UsuarioId,
    Guid CategoriaId,      
    string? Observacoes
) : IRequest<TransacaoDto>;