using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.FechamentoMensal;

public record GetFechamentoMensalByIdQuery(Guid Id) : IRequest<FechamentoMensalDto?>;
